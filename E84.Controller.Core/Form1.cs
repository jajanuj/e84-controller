using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using E84.Controller.Core.Controller;
using E84.Controller.Core.Interfaces;
using E84.Controller.Core.Logging;
using E84.Controller.Core.Mapping;
using E84.Controller.Core.Models;
using E84.Controller.Core.Plc;

namespace E84.Controller.Core
{
   public partial class Form1 : Form
   {
      private E84Controller _controller;
      private FakePlc _fakePlc;
      private IE84IoMap _ioMap;
      private IE84SafetyInterlock _interlock;
      private IE84Logger _logger;
      private E84Config _config;
      private CancellationTokenSource _cts;
      private readonly object _logLock = new object();
      private readonly Queue<string> _logQueue = new Queue<string>();
      private const int MaxLogLines = 1000;

      public Form1()
      {
         InitializeComponent();
         InitializeController();
      }

      private void InitializeController()
      {
         // 初始化 FakePlc 模擬器
         _fakePlc = new FakePlc();

         // Active側(RGV/AGV) I/O 映射表
         // Inputs: 從EQ(Passive)接收的信號
         var inputs = new Dictionary<string, PlcDevice>
         {
            { "L_REQ", new PlcDevice { Address = "X100", Inverted = false } },
            { "U_REQ", new PlcDevice { Address = "X101", Inverted = false } },
            { "READY", new PlcDevice { Address = "X102", Inverted = false } },
            { "LC_REQ", new PlcDevice { Address = "X103", Inverted = false } },
            { "UC_REQ", new PlcDevice { Address = "X104", Inverted = false } },
            { "Carrier", new PlcDevice { Address = "X105", Inverted = false } },
            { "EQ_ONLINE", new PlcDevice { Address = "X106", Inverted = false } },
            { "IN_LINE", new PlcDevice { Address = "X107", Inverted = false } },
            { "ALARM", new PlcDevice { Address = "X108", Inverted = false } },
            { "IDLE", new PlcDevice { Address = "X109", Inverted = false } },
            { "RUN", new PlcDevice { Address = "X110", Inverted = false } }
         };

         // Outputs: RGV/AGV發送給EQ的信號
         var outputs = new Dictionary<string, PlcDevice>
         {
            { "VALID", new PlcDevice { Address = "Y200", Inverted = false } },
            { "TR_REQ", new PlcDevice { Address = "Y201", Inverted = false } },
            { "BUSY", new PlcDevice { Address = "Y202", Inverted = false } },
            { "COMP", new PlcDevice { Address = "Y203", Inverted = false } }
            // Carrier ID 輸出會在實際應用中加入
         };

         _ioMap = new E84IoMap(inputs, outputs, debounceMs: 10);

         // 建立安全互鎖（測試環境永遠通過）
         _interlock = new AlwaysPassInterlock();

         // 建立記錄器
         _logger = new TestUiLogger(this);

         // 建立配置 - Active側超時設定
         _config = new E84Config
         {
            PollIntervalMs = 100,
            DebounceMs = 50,
            SignalResponseDelayMs = 500,  // 0.5秒延遲
            T1_WaitLReqUReqMs = 5000,     // T1: 5秒
            T3_WaitReadyMs = 5000,         // T3: 依現場，預設5秒
            T5_TransferActionMs = 10000,   // T5: 依現場，預設10秒
            T6_WaitReadyOffMs = 5000,      // T6: 5秒
            PlcReconnectDelayMs = 1000,
            PlcReconnectMaxAttempts = 3
         };

         // 建立 E84 控制器
         _controller = new E84Controller(_ioMap, _fakePlc, _interlock, _logger, _config);

         // 註冊事件回呼
         _controller.OnEvent += OnControllerEvent;
      }

      private void OnControllerEvent(E84Event eventType, string message)
      {
         string logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {eventType}: {message}";
         AddLog(logMessage);

         // Auto-clear input signals when returning to Idle after Complete
         // This prevents immediate re-triggering of a new transfer cycle
         if (eventType == E84Event.StateEntered && message == "Idle")
         {
            var status = _controller?.GetStatus();
            if (status != null)
            {
               // Check if we just came from Complete state by checking if this is not the initial Idle
               // We auto-clear the request and handshake signals to simulate proper E84 behavior
               if (InvokeRequired)
               {
                  BeginInvoke(new Action(() => AutoClearInputSignals()));
               }
               else
               {
                  AutoClearInputSignals();
               }
            }
         }
      }

      private void AutoClearInputSignals()
      {
         // Only auto-clear if we're in Idle state (prevents clearing during active transfers)
         var status = _controller?.GetStatus();
         if (status?.State == E84State.Idle)
         {
            // Auto-uncheck signals after completing a transfer
            // This simulates proper E84 protocol where signals are deasserted after handshake
            if (checkBoxL_REQ.Checked)
            {
               checkBoxL_REQ.Checked = false;
               AddLog("[INFO] 自動清除 L_REQ (模擬正常 E84 協定行為)");
            }
            if (checkBoxU_REQ.Checked)
            {
               checkBoxU_REQ.Checked = false;
               AddLog("[INFO] 自動清除 U_REQ (模擬正常 E84 協定行為)");
            }
            if (checkBoxREADY.Checked)
            {
               checkBoxREADY.Checked = false;
               AddLog("[INFO] 自動清除 READY (模擬正常 E84 協定行為)");
            }
            if (checkBoxLC_REQ.Checked)
            {
               checkBoxLC_REQ.Checked = false;
               AddLog("[INFO] 自動清除 LC_REQ (模擬正常 E84 協定行為)");
            }
            if (checkBoxUC_REQ.Checked)
            {
               checkBoxUC_REQ.Checked = false;
               AddLog("[INFO] 自動清除 UC_REQ (模擬正常 E84 協定行為)");
            }
         }
      }

      private void AddLog(string message)
      {
         if (InvokeRequired)
         {
            BeginInvoke(new Action<string>(AddLog), message);
            return;
         }

         lock (_logLock)
         {
            _logQueue.Enqueue(message);
            if (_logQueue.Count > MaxLogLines)
            {
               _logQueue.Dequeue();
            }

            textBoxLog.AppendText(message + Environment.NewLine);
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
         }
      }

      private async void buttonStart_Click(object sender, EventArgs e)
      {
         try
         {
            _cts = new CancellationTokenSource();
            E84Direction direction = radioLoad.Checked ? E84Direction.Load : E84Direction.Unload;

            AddLog($"=== 啟動控制器 Direction: {direction} ===");

            await _controller.StartAsync(direction, _cts.Token);

            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            radioLoad.Enabled = false;
            radioUnload.Enabled = false;
            timerUpdate.Start();
         }
         catch (Exception ex)
         {
            AddLog($"ERROR: 啟動失敗 - {ex.Message}");
            MessageBox.Show($"啟動失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private async void buttonStop_Click(object sender, EventArgs e)
      {
         try
         {
            AddLog("=== 停止控制器 ===");

            _cts?.Cancel();
            await _controller.StopAsync();
            timerUpdate.Stop();

            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            radioLoad.Enabled = true;
            radioUnload.Enabled = true;

            // 重置所有輸入
            foreach (Control c in groupBoxInputs.Controls)
            {
               if (c is CheckBox cb)
               {
                  cb.Checked = false;
               }
            }
         }
         catch (Exception ex)
         {
            AddLog($"ERROR: 停止失敗 - {ex.Message}");
            MessageBox.Show($"停止失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void checkBoxInput_CheckedChanged(object sender, EventArgs e)
      {
         if (_controller == null || _fakePlc == null)
            return;

         CheckBox cb = sender as CheckBox;
         if (cb == null)
            return;

         // 取得信號名稱（移除括號和中文說明）
         string signalName = cb.Text.Split(' ')[0];

         // 取得對應的 PLC 位址
         var inputDevice = _ioMap.GetInput(signalName);
         if (inputDevice != null)
         {
            _fakePlc.SetInput(inputDevice.Value.Address, cb.Checked);
            AddLog($"模擬輸入: {signalName} = {cb.Checked}");
         }
      }

      private void timerUpdate_Tick(object sender, EventArgs e)
      {
         UpdateStatus();
      }

      private void UpdateStatus()
      {
         try
         {
            var status = _controller.GetStatus();

            // 更新狀態標籤
            labelCurrentState.Text = status.State.ToString();

            // 根據狀態改變顏色
            switch (status.State)
            {
               case E84State.Idle:
                  labelCurrentState.ForeColor = Color.Blue;
                  break;
               case E84State.WaitingRequest:
               case E84State.TrReqSent:
               case E84State.ReadyReceived:
               case E84State.Transferring:
                  labelCurrentState.ForeColor = Color.Green;
                  break;
               case E84State.TransferComplete:
               case E84State.WaitingReadyOff:
               case E84State.Complete:
                  labelCurrentState.ForeColor = Color.DarkGreen;
                  break;
               case E84State.Abort:
               case E84State.Error:
                  labelCurrentState.ForeColor = Color.Red;
                  break;
               case E84State.Resetting:
                  labelCurrentState.ForeColor = Color.Orange;
                  break;
            }

            // 更新輸出顯示 - Active側輸出
            UpdateOutputDisplay(labelVALID, status.Outputs.ContainsKey("VALID") && status.Outputs["VALID"]);
            UpdateOutputDisplay(labelTR_REQ, status.Outputs.ContainsKey("TR_REQ") && status.Outputs["TR_REQ"]);
            UpdateOutputDisplay(labelBUSY, status.Outputs.ContainsKey("BUSY") && status.Outputs["BUSY"]);
            UpdateOutputDisplay(labelCOMP, status.Outputs.ContainsKey("COMP") && status.Outputs["COMP"]);

            // 更新狀態資訊
            var statusText = $"State: {status.State}\n" +
                           $"Direction: {status.Direction}\n" +
                           $"Time: {status.TimestampUtc:HH:mm:ss}\n";

            if (!string.IsNullOrEmpty(status.LastError))
            {
               statusText += $"\nLast Error:\n{status.LastError}";
            }

            labelStatusInfo.Text = statusText;
         }
         catch (Exception ex)
         {
            AddLog($"ERROR: 更新狀態失敗 - {ex.Message}");
         }
      }

      private void UpdateOutputDisplay(Label label, bool isActive)
      {
         if (isActive)
         {
            label.BackColor = Color.Lime;
            label.ForeColor = Color.Black;
         }
         else
         {
            label.BackColor = Color.LightGray;
            label.ForeColor = Color.Black;
         }
      }

      private void Form1_Load(object sender, EventArgs e)
      {
         AddLog("=== E84 RGV/AGV控制器測試介面已啟動 ===");
         AddLog("說明：");
         AddLog("1. 選擇方向 (Load/Unload)");
         AddLog("2. 點擊「啟動 Start」開始控制器");
         AddLog("3. 使用左側的核取方塊模擬 EQ端 輸入信號");
         AddLog("4. 觀察右側的 RGV輸出 信號變化");
         AddLog("5. 查看狀態轉換和事件記錄");
         AddLog("");
      }

      private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
      {
         if (_controller != null)
         {
            try
            {
               _cts?.Cancel();
               await _controller.StopAsync();
               timerUpdate.Stop();
            }
            catch
            {
               // 忽略關閉時的錯誤
            }
         }
      }

      /// <summary>
      /// 自訂 Logger 實作，將訊息輸出到 UI
      /// </summary>
      private class TestUiLogger : IE84Logger
      {
         private readonly Form1 _form;

         public TestUiLogger(Form1 form)
         {
            _form = form;
         }

         public void Debug(string message)
         {
            _form.AddLog($"[DEBUG] {message}");
         }

         public void Info(string message)
         {
            _form.AddLog($"[INFO] {message}");
         }

         public void Warn(string message)
         {
            _form.AddLog($"[WARN] {message}");
         }

         public void Error(string message, Exception ex = null)
         {
            string errorMsg = ex != null ? $"{message} - {ex.Message}" : message;
            _form.AddLog($"[ERROR] {errorMsg}");
         }
      }
   }
}
