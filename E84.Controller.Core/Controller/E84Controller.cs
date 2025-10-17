using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using E84.Controller.Core.Exceptions;
using E84.Controller.Core.Interfaces;
using E84.Controller.Core.Logging;
using E84.Controller.Core.Models;

namespace E84.Controller.Core.Controller
{
   /// <summary>
   /// E84 狀態機控制核心。
   /// 公開 API:
   /// - StartAsync(direction, ct)：開始背景輪詢與處理。
   /// - StopAsync()：請求停止並等待背景迴圈結束。
   /// - GetStatus()：回傳目前狀態快照與 I/O。
   ///
   /// 狀態機語意:
   /// - 啟動時：確保 PLC 連線並將輸出初始化為安全預設（全部 false）。
   /// - 持續執行循環：ReadInputs -> StepState -> WriteOutputs -> Delay(poll)。
   /// - 每個狀態在進入/離開時記錄日誌，並處理逾時與透過 IE84SafetyInterlock 做互鎖檢查。
   /// - 逾時或互鎖失敗會導致 Error/Abort 流程（包含 ABORT 輸出宣告）。
   /// </summary>
   public class E84Controller
   {
      #region Fields

      private readonly E84Config _config;
      private readonly Dictionary<string, DateTime> _inputLastChanged = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

      private readonly Dictionary<string, bool> _inputStable = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
      private readonly IE84SafetyInterlock _interlock;
      private readonly IE84IoMap _ioMap;
      private readonly IE84Logger _logger;
      private readonly Dictionary<string, bool> _outputState = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
      private readonly IE84Plc _plc;
      private readonly Stopwatch _sw = Stopwatch.StartNew();
      private readonly object _sync = new object();
      private Task _backgroundTask;

      private CancellationTokenSource _cts;
      private E84Direction _direction = E84Direction.Inbound;
      private Exception _lastError;

      private E84State _state = E84State.Idle;
      private DateTime _stateEnteredAt = DateTime.UtcNow;

      /// <summary>
      /// 控制器事件回呼簽章。
      /// </summary>
      public Action<E84Event, string> OnEvent;

      #endregion

      #region Constructors

      /// <summary>
      /// 建構 E84Controller。
      /// </summary>
      public E84Controller(IE84IoMap ioMap, IE84Plc plc, IE84SafetyInterlock interlock, IE84Logger logger, E84Config config)
      {
         _ioMap = ioMap ?? throw new ArgumentNullException(nameof(ioMap));
         _plc = plc ?? throw new ArgumentNullException(nameof(plc));
         _interlock = interlock ?? throw new ArgumentNullException(nameof(interlock));
         _logger = logger ?? new ConsoleE84Logger();
         _config = config ?? new E84Config();
         // initialize outputs snapshot
         foreach (var kv in _ioMap.Outputs)
         {
            _outputState[kv.Key] = false;
         }

         foreach (var kv in _ioMap.Inputs)
         {
            _inputStable[kv.Key] = false;
            _inputLastChanged[kv.Key] = DateTime.UtcNow;
         }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// 以指定方向啟動控制器背景處理。
      /// 此方法非阻塞；若已在執行中，會允許切換方向而不中斷迴圈。
      /// </summary>
      public async Task StartAsync(E84Direction direction, CancellationToken ct)
      {
         lock (_sync)
         {
            if (_cts != null)
            {
               // already running - allow direction change
               _direction = direction;
               _logger.Info($"Direction changed to {_direction}");
               return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _direction = direction;
            _state = E84State.Idle;
            _stateEnteredAt = DateTime.UtcNow;
            _lastError = null;
            _cts.Token.ThrowIfCancellationRequested();

            _backgroundTask = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
         }

         // optionally return once started
         await Task.CompletedTask;
      }

      /// <summary>
      /// 平滑停止控制器並等待背景任務結束。
      /// </summary>
      public async Task StopAsync()
      {
         lock (_sync)
         {
            if (_cts == null) return;
            _cts.Cancel();
         }

         try
         {
            if (_backgroundTask != null) await _backgroundTask.ConfigureAwait(false);
         }
         catch (OperationCanceledException)
         {
         }
         finally
         {
            lock (_sync)
            {
               _cts?.Dispose();
               _cts = null;
               _backgroundTask = null;
            }
         }
      }

      /// <summary>
      /// 取得控制器目前狀態快照。
      /// </summary>
      public E84StatusSnapshot GetStatus()
      {
         var inputs = _ioMap.Inputs.Keys.ToDictionary(k => k, k => _inputStable.TryGetValue(k, out bool v) ? v : false, StringComparer.OrdinalIgnoreCase);
         var outputs = _ioMap.Outputs.Keys.ToDictionary(k => k, k => _outputState.TryGetValue(k, out bool v) ? v : false, StringComparer.OrdinalIgnoreCase);

         return new E84StatusSnapshot
         {
            State = _state,
            Direction = _direction,
            Inputs = inputs,
            Outputs = outputs,
            LastError = _lastError?.Message,
            TimestampUtc = DateTime.UtcNow
         };
      }

      #endregion

      #region Private Methods

      private async Task RunAsync(CancellationToken ct)
      {
         // Ensure PLC is connected with configured retries
         try
         {
            _plc.EnsureConnected(_config.PlcReconnectMaxAttempts, _config.PlcReconnectDelayMs);
            _logger.Info("PLC connected");
            OnEvent?.Invoke(E84Event.PlcReconnected, "PLC connected");
         }
         catch (PlcException ex)
         {
            _logger.Error("Initial PLC connection failed", ex);
            _lastError = ex;
            OnEvent?.Invoke(E84Event.PlcDisconnected, "Initial PLC connect failed");
            // continue and enter error state
            TransitionTo(E84State.Error);
         }

         // initialize outputs to safe state
         SetAllOutputsSafe();

         while (!ct.IsCancellationRequested)
         {
            try
            {
               if (!_plc.IsConnected)
               {
                  OnEvent?.Invoke(E84Event.PlcDisconnected, "PLC disconnected during run");
                  _logger.Warn("PLC disconnected — attempting reconnect");
                  try
                  {
                     _plc.EnsureConnected(_config.PlcReconnectMaxAttempts, _config.PlcReconnectDelayMs);
                     _logger.Info("PLC reconnected");
                     OnEvent?.Invoke(E84Event.PlcReconnected, "PLC reconnected");
                  }
                  catch (PlcException rex)
                  {
                     _logger.Error("PLC reconnect failed", rex);
                     _lastError = rex;
                     // keep outputs safe and sleep then continue polling for reconnection attempts
                     SetAllOutputsSafe();
                     await Task.Delay(_config.PlcReconnectDelayMs, ct).ConfigureAwait(false);
                     continue;
                  }
               }

               ReadInputsWithDebounce();
               Step(ct);
               WriteOutputs();
            }
            catch (PlcException pex)
            {
               _logger.Error("PLC exception", pex);
               _lastError = pex;
               // mark disconnected and try reconnect next cycle
               try
               {
                  _plc.Close();
               }
               catch
               {
               }
            }
            catch (E84InterlockException iex)
            {
               _logger.Warn($"Interlock failed: {iex.InterlockName}");
               _lastError = iex;
               OnEvent?.Invoke(E84Event.InterlockFailed, iex.InterlockName);
               // assert abort output
               SetOutputState("ABORT", true);
               TransitionTo(E84State.Abort);
            }
            catch (E84TimeoutException tex)
            {
               _logger.Error($"Timeout: {tex.Message}");
               _lastError = tex;
               OnEvent?.Invoke(E84Event.Timeout, tex.Message);
               // transition to Error/Abort
               SetOutputState("ABORT", true);
               TransitionTo(E84State.Abort);
            }
            catch (OperationCanceledException)
            {
               throw;
            }
            catch (Exception ex)
            {
               _logger.Error("Unhandled controller exception", ex);
               _lastError = ex;
               TransitionTo(E84State.Error);
            }

            await Task.Delay(_config.PollIntervalMs, ct).ConfigureAwait(false);
         }

         // On exit, reset outputs to safe
         SetAllOutputsSafe();
         _logger.Info("Controller loop stopped");
      }

      private void ReadInputsWithDebounce()
      {
         foreach (var kv in _ioMap.Inputs)
         {
            string name = kv.Key;
            var dev = kv.Value;
            bool raw;
            try
            {
               raw = _plc.ReadBit(dev.Address);
            }
            catch (PlcException)
            {
               throw;
            }

            // apply inversion from mapping
            if (dev.Inverted) raw = !raw;

            // debounce logic
            if (!_inputStable.ContainsKey(name))
            {
               _inputStable[name] = raw;
               _inputLastChanged[name] = DateTime.UtcNow;
               continue;
            }

            bool last = _inputStable[name];
            if (raw != last)
            {
               // start debounce timer
               var lastChange = _inputLastChanged[name];
               if ((DateTime.UtcNow - lastChange).TotalMilliseconds >= _config.DebounceMs)
               {
                  // commit change
                  _inputStable[name] = raw;
                  _inputLastChanged[name] = DateTime.UtcNow;
                  if (raw)
                  {
                     _logger.Debug($"Signal rising: {name}");
                     OnEvent?.Invoke(E84Event.SignalRising, name);
                  }
                  else
                  {
                     _logger.Debug($"Signal falling: {name}");
                     OnEvent?.Invoke(E84Event.SignalFalling, name);
                  }
               }
               else
               {
                  // update last changed time to current to continue stable period requirement
                  _inputLastChanged[name] = DateTime.UtcNow;
               }
            }
            else
            {
               // stable, update last changed to now so next flip needs debounce
               _inputLastChanged[name] = DateTime.UtcNow;
            }
         }
      }

      private void Step(CancellationToken ct)
      {
         // check interlocks for current state
         if (!_interlock.Check(_state, out string reason))
         {
            throw new E84InterlockException(reason);
         }

         switch (_state)
         {
            case E84State.Idle:
               // Wait for incoming request depending on direction
               if (_direction == E84Direction.Inbound)
               {
                  if (GetInput("TR_REQ"))
                  {
                     TransitionTo(E84State.Request);
                  }
               }
               else
               {
                  // Outbound triggered by L_REQ/U_REQ
                  if (GetInput("L_REQ") || GetInput("U_REQ"))
                  {
                     TransitionTo(E84State.Request);
                  }
               }

               break;

            case E84State.Request:
               // assert BUSY/HO_AVBL or CLAMP according to direction
               if (_direction == E84Direction.Inbound)
               {
                  SetOutputState("HO_AVBL", true); // tell AMHS we are available for handoff
                  SetOutputState("BUSY", true);
                  TransitionTo(E84State.Busy);
               }
               else
               {
                  // Outbound: clamp then dock
                  SetOutputState("CLAMP", true);
                  // wait clamp action time
                  if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.ClampMs)
                  {
                     TransitionTo(E84State.Busy);
                  }
               }

               break;

            case E84State.Busy:
               if (_direction == E84Direction.Inbound)
               {
                  // wait for VALID from partner
                  if (GetInput("VALID"))
                  {
                     TransitionTo(E84State.Valid);
                  }
                  else if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.WaitValidMs)
                  {
                     throw new E84TimeoutException(E84FaultCode.TimeoutWaitValid, _state, "VALID");
                  }
               }
               else
               {
                  // Outbound: Dock action
                  SetOutputState("DOCK", true);
                  if (GetInput("READY"))
                  {
                     TransitionTo(E84State.Valid);
                  }
                  else if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.DockMs)
                  {
                     throw new E84TimeoutException(E84FaultCode.TimeoutWaitTrReq, _state, "READY");
                  }
               }

               break;

            case E84State.Valid:
               // perform transfer: assert TRANSFER and wait COMPT
               SetOutputState("TRANSFER", true);
               TransitionTo(E84State.Transfer);
               break;

            case E84State.Transfer:
               // wait for COMPT
               if (GetInput("COMPT"))
               {
                  TransitionTo(E84State.Complete);
               }
               else if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.WaitComptMs)
               {
                  throw new E84TimeoutException(E84FaultCode.TimeoutWaitCompt, _state, "COMPT");
               }

               break;

            case E84State.Complete:
               // clear TRANSFER and BUSY/CLAMP/DOCK and go back to Idle
               SetOutputState("TRANSFER", false);
               SetOutputState("BUSY", false);
               SetOutputState("HO_AVBL", false);
               SetOutputState("CLAMP", false);
               SetOutputState("DOCK", false);
               TransitionTo(E84State.Idle);
               break;

            case E84State.Abort:
               // keep ABORT asserted until RESET input is observed or manual reset
               if (GetInput("RESET"))
               {
                  SetOutputState("ABORT", false);
                  TransitionTo(E84State.Resetting);
               }

               break;

            case E84State.Resetting:
               // wait a short time then go Idle
               if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > 250)
               {
                  TransitionTo(E84State.Idle);
               }

               break;

            case E84State.Error:
               // remain until external reset
               break;
         }
      }

      private bool GetInput(string logicalName)
      {
         if (!_inputStable.TryGetValue(logicalName, out bool v)) return false;
         return v;
      }

      private void SetOutputState(string logicalName, bool value)
      {
         if (!_outputState.ContainsKey(logicalName))
         {
            _logger.Warn($"Attempt to set unknown output {logicalName}");
            return;
         }

         bool prev = _outputState[logicalName];
         if (prev == value) return;

         var dev = _ioMap.GetOutput(logicalName);
         if (dev == null)
         {
            _logger.Warn($"Output mapping not found for {logicalName}");
            _outputState[logicalName] = value;
            return;
         }

         try
         {
            // apply inversion at write-time: if mapping says Inverted, flip logical value
            bool writeValue = dev.Value.Inverted ? !value : value;
            _plc.WriteBit(dev.Value.Address, writeValue);
            _outputState[logicalName] = value;
            _logger.Debug($"Output {logicalName} -> {value} (wrote {writeValue} to {dev.Value.Address})");
         }
         catch (PlcException pex)
         {
            _logger.Error($"WriteBit failed for {logicalName}", pex);
            throw;
         }
      }

      private void WriteOutputs()
      {
         // Ensure outputs stored in _outputState are actually written — this method can be used for batch writes later.
         // Currently writes were already executed in SetOutputState; consider extending to batch writes to reduce PLC chatter.
      }

      private void SetAllOutputsSafe()
      {
         foreach (var kv in _ioMap.Outputs)
         {
            try
            {
               var name = kv.Key;
               var dev = kv.Value;
               bool safe = false;
               bool writeValue = dev.Inverted ? !safe : safe;
               if (_plc.IsConnected)
               {
                  _plc.WriteBit(dev.Address, writeValue);
               }

               _outputState[name] = safe;
            }
            catch
            {
               // swallow to ensure best-effort safe outputs
            }
         }
      }

      private void TransitionTo(E84State newState)
      {
         var prev = _state;
         if (prev == newState) return;
         _logger.Info($"{prev} -> {newState}");
         OnEvent?.Invoke(E84Event.StateLeft, prev.ToString());
         _state = newState;
         _stateEnteredAt = DateTime.UtcNow;
         OnEvent?.Invoke(E84Event.StateEntered, newState.ToString());
      }

      #endregion
   }
}