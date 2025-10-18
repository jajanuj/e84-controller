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
   /// E84 狀態機控制核心 - Active側(RGV/AGV)實作。
   /// 
   /// 公開 API:
   /// - StartAsync(direction, ct)：開始背景輪詢與處理。
   /// - StopAsync()：請求停止並等待背景迴圈結束。
   /// - GetStatus()：回傳目前狀態快照與 I/O。
   /// 
   /// 實作規格:
   /// - RGV/AGV作為Active側，發送VALID, TR_REQ, BUSY, COMP
   /// - 接收EQ端(Passive)的L_REQ, U_REQ, READY, LC_REQ, UC_REQ等信號
   /// - 所有交握訊號延遲0.5秒回覆
   /// - 監控T1, T3, T5, T6超時
   /// - 異常檢知與處理
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
      private E84Direction _direction = E84Direction.Load;
      private Exception _lastError;

      private E84State _state = E84State.Idle;
      private DateTime _stateEnteredAt = DateTime.UtcNow;
      
      // 延遲發送信號的時間戳
      private DateTime? _delayedSignalTime = null;
      private Action _delayedSignalAction = null;

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
            _delayedSignalTime = null;
            _delayedSignalAction = null;
            _cts.Token.ThrowIfCancellationRequested();

            _backgroundTask = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
         }

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
         // Ensure PLC is connected
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
                     SetAllOutputsSafe();
                     await Task.Delay(_config.PlcReconnectDelayMs, ct).ConfigureAwait(false);
                     continue;
                  }
               }

               ReadInputsWithDebounce();
               CheckErrorConditions();
               Step(ct);
               ProcessDelayedSignal();
               WriteOutputs();
            }
            catch (PlcException pex)
            {
               _logger.Error("PLC exception", pex);
               _lastError = pex;
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
               TransitionTo(E84State.Abort);
            }
            catch (E84TimeoutException tex)
            {
               _logger.Error($"Timeout: {tex.Message}");
               _lastError = tex;
               OnEvent?.Invoke(E84Event.Timeout, tex.Message);
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
                  _inputLastChanged[name] = DateTime.UtcNow;
               }
            }
            else
            {
               _inputLastChanged[name] = DateTime.UtcNow;
            }
         }
      }

      /// <summary>
      /// 檢查Active側的異常條件
      /// </summary>
      private void CheckErrorConditions()
      {
         // (5) L_REQ和U_REQ同時ON
         if (GetInput("L_REQ") && GetInput("U_REQ"))
         {
            _logger.Error("Error: Both L_REQ and U_REQ are ON");
            throw new E84TimeoutException(E84FaultCode.ErrorBothLReqUReqOn, _state, "L_REQ and U_REQ both ON");
         }

         // (6) TR_REQ未ON，READY先ON
         if (!GetOutput("TR_REQ") && GetInput("READY"))
         {
            _logger.Error("Error: READY ON before TR_REQ");
            throw new E84TimeoutException(E84FaultCode.ErrorReadyBeforeTrReq, _state, "READY before TR_REQ");
         }

         // (9) BUSY ON時，READY OFF
         if (GetOutput("BUSY") && !GetInput("READY"))
         {
            // 例外：在TransferComplete狀態時READY可以OFF (這是正常流程)
            if (_state != E84State.TransferComplete && _state != E84State.WaitingReadyOff && _state != E84State.Complete)
            {
               _logger.Error("Error: READY OFF while BUSY ON");
               throw new E84TimeoutException(E84FaultCode.ErrorReadyOffDuringBusy, _state, "READY OFF during BUSY");
            }
         }

         // (10) 移動到EQ Port時，LC_REQ或UC_REQ OFF (在WaitingRequest或之後的狀態檢查)
         if (_state != E84State.Idle && _state != E84State.Abort && _state != E84State.Error && _state != E84State.Resetting)
         {
            bool lcReq = GetInput("LC_REQ");
            bool ucReq = GetInput("UC_REQ");
            
            if (_direction == E84Direction.Load && !lcReq)
            {
               _logger.Error("Error: LC_REQ OFF during Load transfer");
               throw new E84TimeoutException(E84FaultCode.ErrorLcUcReqOff, _state, "LC_REQ OFF");
            }
            else if (_direction == E84Direction.Unload && !ucReq)
            {
               _logger.Error("Error: UC_REQ OFF during Unload transfer");
               throw new E84TimeoutException(E84FaultCode.ErrorLcUcReqOff, _state, "UC_REQ OFF");
            }
         }

         // 檢查EQ_ONLINE狀態 - 如果OFF則結束交握
         if (!GetInput("EQ_ONLINE"))
         {
            if (_state != E84State.Idle && _state != E84State.Error)
            {
               _logger.Warn("EQ_ONLINE is OFF, ending handshake");
               TransitionTo(E84State.Abort);
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
               // 等待來自LCS的搬送命令(由外部觸發LC_REQ或UC_REQ)
               // Load: 檢查LC_REQ
               // Unload: 檢查UC_REQ
               if (_direction == E84Direction.Load && GetInput("LC_REQ"))
               {
                  // RGV接收到LC_REQ，準備移動到EQ位置並發送Carrier ID和VALID
                  _logger.Info("LC_REQ received, starting Load sequence");
                  // 延遲0.5秒後發送VALID
                  ScheduleDelayedSignal(() =>
                  {
                     SetOutputState("VALID", true);
                     TransitionTo(E84State.WaitingRequest);
                  });
               }
               else if (_direction == E84Direction.Unload && GetInput("UC_REQ"))
               {
                  // RGV接收到UC_REQ (EQ已發送Carrier ID)，準備移動到EQ位置並發送VALID
                  _logger.Info("UC_REQ received, starting Unload sequence");
                  // 延遲0.5秒後發送VALID
                  ScheduleDelayedSignal(() =>
                  {
                     SetOutputState("VALID", true);
                     TransitionTo(E84State.WaitingRequest);
                  });
               }
               break;

            case E84State.WaitingRequest:
               // 已發送VALID，等待EQ的L_REQ或U_REQ (T1超時監控)
               if (_direction == E84Direction.Load && GetInput("L_REQ"))
               {
                  _logger.Info("L_REQ received");
                  // 延遲0.5秒後發送TR_REQ
                  ScheduleDelayedSignal(() =>
                  {
                     SetOutputState("TR_REQ", true);
                     TransitionTo(E84State.TrReqSent);
                  });
               }
               else if (_direction == E84Direction.Unload && GetInput("U_REQ"))
               {
                  _logger.Info("U_REQ received");
                  // 延遲0.5秒後發送TR_REQ
                  ScheduleDelayedSignal(() =>
                  {
                     SetOutputState("TR_REQ", true);
                     TransitionTo(E84State.TrReqSent);
                  });
               }
               else if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.T1_WaitLReqUReqMs)
               {
                  // T1超時
                  throw new E84TimeoutException(E84FaultCode.TimeoutT1_WaitLReqUReq, _state, "L_REQ/U_REQ");
               }
               break;

            case E84State.TrReqSent:
               // 已發送TR_REQ，等待EQ的READY (T3超時監控)
               if (GetInput("READY"))
               {
                  _logger.Info("READY received");
                  // 延遲0.5秒後發送BUSY
                  ScheduleDelayedSignal(() =>
                  {
                     SetOutputState("BUSY", true);
                     TransitionTo(E84State.ReadyReceived);
                  });
               }
               else if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.T3_WaitReadyMs)
               {
                  // T3超時
                  throw new E84TimeoutException(E84FaultCode.TimeoutT3_WaitReady, _state, "READY");
               }
               break;

            case E84State.ReadyReceived:
               // BUSY已ON，立即開始搬運動作
               TransitionTo(E84State.Transferring);
               break;

            case E84State.Transferring:
               // 模擬搬運動作 - 實際應用中這裡會與RGV的動作系統整合
               // T5: 監控搬運動作時間
               if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.T5_TransferActionMs)
               {
                  // 搬運完成，離開交握區
                  _logger.Info("Transfer action complete, leaving handshake area");
                  SetOutputState("BUSY", false);
                  TransitionTo(E84State.TransferComplete);
               }
               break;

            case E84State.TransferComplete:
               // BUSY已OFF，發送COMP
               ScheduleDelayedSignal(() =>
               {
                  SetOutputState("COMP", true);
                  TransitionTo(E84State.WaitingReadyOff);
               });
               break;

            case E84State.WaitingReadyOff:
               // 已發送COMP，等待EQ的READY OFF (T6超時監控)
               if (!GetInput("READY"))
               {
                  _logger.Info("READY OFF received");
                  // 延遲0.5秒後COMP OFF
                  ScheduleDelayedSignal(() =>
                  {
                     SetOutputState("COMP", false);
                     TransitionTo(E84State.Complete);
                  });
               }
               else if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > _config.T6_WaitReadyOffMs)
               {
                  // T6超時
                  throw new E84TimeoutException(E84FaultCode.TimeoutT6_WaitReadyOff, _state, "READY OFF");
               }
               break;

            case E84State.Complete:
               // COMP已OFF，VALID OFF，返回Idle
               SetOutputState("VALID", false);
               SetOutputState("TR_REQ", false);
               TransitionTo(E84State.Idle);
               break;

            case E84State.Abort:
               // 異常狀態 - 根據規格處理
               // 如果BUSY未ON，所有信號OFF
               // 如果BUSY已ON，等待離開交握區後BUSY OFF，其他信號OFF
               if (GetOutput("BUSY"))
               {
                  _logger.Info("Abort during BUSY - waiting to leave handshake area");
                  // 實際應用中應該等待物理位置離開交握區
                  // 這裡簡化為延遲後BUSY OFF
                  if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > 2000)
                  {
                     SetOutputState("BUSY", false);
                  }
               }
               
               // 清除所有信號
               SetOutputState("VALID", false);
               SetOutputState("TR_REQ", false);
               SetOutputState("COMP", false);
               
               // 等待手動重置或條件恢復
               if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > 1000)
               {
                  TransitionTo(E84State.Resetting);
               }
               break;

            case E84State.Resetting:
               // 復原後返回Idle
               if ((DateTime.UtcNow - _stateEnteredAt).TotalMilliseconds > 500)
               {
                  _lastError = null;
                  TransitionTo(E84State.Idle);
               }
               break;

            case E84State.Error:
               // 保持在錯誤狀態直到外部重置
               break;
         }
      }

      /// <summary>
      /// 排程延遲0.5秒的信號動作
      /// </summary>
      private void ScheduleDelayedSignal(Action action)
      {
         _delayedSignalTime = DateTime.UtcNow.AddMilliseconds(_config.SignalResponseDelayMs);
         _delayedSignalAction = action;
      }

      /// <summary>
      /// 處理延遲信號
      /// </summary>
      private void ProcessDelayedSignal()
      {
         if (_delayedSignalTime.HasValue && _delayedSignalAction != null)
         {
            if (DateTime.UtcNow >= _delayedSignalTime.Value)
            {
               _delayedSignalAction.Invoke();
               _delayedSignalTime = null;
               _delayedSignalAction = null;
            }
         }
      }

      private bool GetInput(string logicalName)
      {
         if (!_inputStable.TryGetValue(logicalName, out bool v)) return false;
         return v;
      }

      private bool GetOutput(string logicalName)
      {
         if (!_outputState.TryGetValue(logicalName, out bool v)) return false;
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
         // Outputs are written immediately in SetOutputState
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