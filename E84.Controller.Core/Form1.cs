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

         // 建立 I/O 映射表
         var inputs = new Dictionary<string, PlcDevice>
         {
            { "TR_REQ", new PlcDevice { Address = "X100", Inverted = false } },
            { "VALID", new PlcDevice { Address = "X101", Inverted = false } },
            { "COMPT", new PlcDevice { Address = "X102", Inverted = false } },
            { "L_REQ", new PlcDevice { Address = "X103", Inverted = false } },
            { "U_REQ", new PlcDevice { Address = "X104", Inverted = false } },
            { "READY", new PlcDevice { Address = "X105", Inverted = false } },
            { "RESET", new PlcDevice { Address = "X106", Inverted = false } }
         };

         var outputs = new Dictionary<string, PlcDevice>
         {
            { "BUSY", new PlcDevice { Address = "Y200", Inverted = false } },
            { "HO_AVBL", new PlcDevice { Address = "Y201", Inverted = false } },
            { "TRANSFER", new PlcDevice { Address = "Y202", Inverted = false } },
            { "CLAMP", new PlcDevice { Address = "Y203", Inverted = false } },
            { "DOCK", new PlcDevice { Address = "Y204", Inverted = false } },
            { "ABORT", new PlcDevice { Address = "Y205", Inverted = false } }
         };

         _ioMap = new E84IoMap(inputs, outputs, debounceMs: 10);

         // 建立安全互鎖（測試環境永遠通過）
         _interlock = new AlwaysPassInterlock();

         // 建立記錄器
         _logger = new TestUiLogger(this);

         // 建立配置
         _config = new E84Config
         {
            PollIntervalMs = 100,
            DebounceMs = 50,
            WaitTrReqMs = 10000,
            WaitValidMs = 10000,
            WaitComptMs = 15000,
            ClampMs = 1500,
            DockMs = 3000,
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
            // Auto-uncheck TR_REQ, COMPT after completing a transfer
            // This simulates proper E84 protocol where signals are deasserted after handshake
            if (checkBoxTR_REQ.Checked)
            {
               checkBoxTR_REQ.Checked = false;
               AddLog("[INFO] 自動清除 TR_REQ (模擬正常 E84 協定行為)");
            }
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
            if (checkBoxCOMPT.Checked)
            {
               checkBoxCOMPT.Checked = false;
               AddLog("[INFO] 自動清除 COMPT (模擬正常 E84 協定行為)");
            }
            if (checkBoxVALID.Checked)
            {
               checkBoxVALID.Checked = false;
               AddLog("[INFO] 自動清除 VALID (模擬正常 E84 協定行為)");
            }
            if (checkBoxREADY.Checked)
            {
               checkBoxREADY.Checked = false;
               AddLog("[INFO] 自動清除 READY (模擬正常 E84 協定行為)");
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
            E84Direction direction = radioInbound.Checked ? E84Direction.Inbound : E84Direction.Outbound;

            AddLog($"=== 啟動控制器 Direction: {direction} ===");

            await _controller.StartAsync(direction, _cts.Token);

            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            radioInbound.Enabled = false;
            radioOutbound.Enabled = false;
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
            radioInbound.Enabled = true;
            radioOutbound.Enabled = true;

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
               case E84State.Request:
               case E84State.Busy:
               case E84State.Valid:
               case E84State.Transfer:
                  labelCurrentState.ForeColor = Color.Green;
                  break;
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

            // 更新輸出顯示
            UpdateOutputDisplay(labelBUSY, status.Outputs.ContainsKey("BUSY") && status.Outputs["BUSY"]);
            UpdateOutputDisplay(labelHO_AVBL, status.Outputs.ContainsKey("HO_AVBL") && status.Outputs["HO_AVBL"]);
            UpdateOutputDisplay(labelTRANSFER, status.Outputs.ContainsKey("TRANSFER") && status.Outputs["TRANSFER"]);
            UpdateOutputDisplay(labelCLAMP, status.Outputs.ContainsKey("CLAMP") && status.Outputs["CLAMP"]);
            UpdateOutputDisplay(labelDOCK, status.Outputs.ContainsKey("DOCK") && status.Outputs["DOCK"]);
            UpdateOutputDisplay(labelABORT, status.Outputs.ContainsKey("ABORT") && status.Outputs["ABORT"]);

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
         AddLog("=== E84 控制器測試介面已啟動 ===");
         AddLog("說明：");
         AddLog("1. 選擇方向 (Inbound/Outbound)");
         AddLog("2. 點擊「啟動 Start」開始控制器");
         AddLog("3. 使用左側的核取方塊模擬 PLC 輸入信號");
         AddLog("4. 觀察右側的輸出信號變化");
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
