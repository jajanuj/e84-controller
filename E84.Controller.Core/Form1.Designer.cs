namespace E84.Controller.Core
{
   partial class Form1
   {
      /// <summary>
      /// 設計工具所需的變數。
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// 清除任何使用中的資源。
      /// </summary>
      /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form 設計工具產生的程式碼

      /// <summary>
      /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
      /// 這個方法的內容。
      /// </summary>
      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         this.groupBoxController = new System.Windows.Forms.GroupBox();
         this.labelCurrentState = new System.Windows.Forms.Label();
         this.labelState = new System.Windows.Forms.Label();
         this.buttonStop = new System.Windows.Forms.Button();
         this.buttonStart = new System.Windows.Forms.Button();
         this.radioUnload = new System.Windows.Forms.RadioButton();
         this.radioLoad = new System.Windows.Forms.RadioButton();
         this.labelDirection = new System.Windows.Forms.Label();
         this.groupBoxInputs = new System.Windows.Forms.GroupBox();
         this.checkBoxRUN = new System.Windows.Forms.CheckBox();
         this.checkBoxIDLE = new System.Windows.Forms.CheckBox();
         this.checkBoxALARM = new System.Windows.Forms.CheckBox();
         this.checkBoxIN_LINE = new System.Windows.Forms.CheckBox();
         this.checkBoxEQ_ONLINE = new System.Windows.Forms.CheckBox();
         this.checkBoxCarrier = new System.Windows.Forms.CheckBox();
         this.checkBoxUC_REQ = new System.Windows.Forms.CheckBox();
         this.checkBoxLC_REQ = new System.Windows.Forms.CheckBox();
         this.checkBoxREADY = new System.Windows.Forms.CheckBox();
         this.checkBoxU_REQ = new System.Windows.Forms.CheckBox();
         this.checkBoxL_REQ = new System.Windows.Forms.CheckBox();
         this.groupBoxOutputs = new System.Windows.Forms.GroupBox();
         this.labelCOMP = new System.Windows.Forms.Label();
         this.labelBUSY = new System.Windows.Forms.Label();
         this.labelTR_REQ = new System.Windows.Forms.Label();
         this.labelVALID = new System.Windows.Forms.Label();
         this.groupBoxLog = new System.Windows.Forms.GroupBox();
         this.textBoxLog = new System.Windows.Forms.TextBox();
         this.timerUpdate = new System.Windows.Forms.Timer(this.components);
         this.groupBoxStatus = new System.Windows.Forms.GroupBox();
         this.labelStatusInfo = new System.Windows.Forms.Label();
         this.groupBoxController.SuspendLayout();
         this.groupBoxInputs.SuspendLayout();
         this.groupBoxOutputs.SuspendLayout();
         this.groupBoxLog.SuspendLayout();
         this.groupBoxStatus.SuspendLayout();
         this.SuspendLayout();
         // 
         // groupBoxController
         // 
         this.groupBoxController.Controls.Add(this.labelCurrentState);
         this.groupBoxController.Controls.Add(this.labelState);
         this.groupBoxController.Controls.Add(this.buttonStop);
         this.groupBoxController.Controls.Add(this.buttonStart);
         this.groupBoxController.Controls.Add(this.radioUnload);
         this.groupBoxController.Controls.Add(this.radioLoad);
         this.groupBoxController.Controls.Add(this.labelDirection);
         this.groupBoxController.Location = new System.Drawing.Point(12, 12);
         this.groupBoxController.Name = "groupBoxController";
         this.groupBoxController.Size = new System.Drawing.Size(300, 150);
         this.groupBoxController.TabIndex = 0;
         this.groupBoxController.TabStop = false;
         this.groupBoxController.Text = "RGV控制器 Controller";
         // 
         // labelCurrentState
         // 
         this.labelCurrentState.AutoSize = true;
         this.labelCurrentState.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.labelCurrentState.ForeColor = System.Drawing.Color.Blue;
         this.labelCurrentState.Location = new System.Drawing.Point(90, 110);
         this.labelCurrentState.Name = "labelCurrentState";
         this.labelCurrentState.Size = new System.Drawing.Size(41, 20);
         this.labelCurrentState.TabIndex = 6;
         this.labelCurrentState.Text = "Idle";
         // 
         // labelState
         // 
         this.labelState.AutoSize = true;
         this.labelState.Location = new System.Drawing.Point(15, 115);
         this.labelState.Name = "labelState";
         this.labelState.Size = new System.Drawing.Size(69, 13);
         this.labelState.TabIndex = 5;
         this.labelState.Text = "狀態 State:";
         // 
         // buttonStop
         // 
         this.buttonStop.Enabled = false;
         this.buttonStop.Location = new System.Drawing.Point(185, 73);
         this.buttonStop.Name = "buttonStop";
         this.buttonStop.Size = new System.Drawing.Size(90, 25);
         this.buttonStop.TabIndex = 4;
         this.buttonStop.Text = "停止 Stop";
         this.buttonStop.UseVisualStyleBackColor = true;
         this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
         // 
         // buttonStart
         // 
         this.buttonStart.Location = new System.Drawing.Point(185, 42);
         this.buttonStart.Name = "buttonStart";
         this.buttonStart.Size = new System.Drawing.Size(90, 25);
         this.buttonStart.TabIndex = 3;
         this.buttonStart.Text = "啟動 Start";
         this.buttonStart.UseVisualStyleBackColor = true;
         this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
         // 
         // radioUnload
         // 
         this.radioUnload.AutoSize = true;
         this.radioUnload.Location = new System.Drawing.Point(18, 66);
         this.radioUnload.Name = "radioUnload";
         this.radioUnload.Size = new System.Drawing.Size(105, 17);
         this.radioUnload.TabIndex = 2;
         this.radioUnload.Text = "Unload (出料)";
         this.radioUnload.UseVisualStyleBackColor = true;
         // 
         // radioLoad
         // 
         this.radioLoad.AutoSize = true;
         this.radioLoad.Checked = true;
         this.radioLoad.Location = new System.Drawing.Point(18, 43);
         this.radioLoad.Name = "radioLoad";
         this.radioLoad.Size = new System.Drawing.Size(92, 17);
         this.radioLoad.TabIndex = 1;
         this.radioLoad.TabStop = true;
         this.radioLoad.Text = "Load (入料)";
         this.radioLoad.UseVisualStyleBackColor = true;
         // 
         // labelDirection
         // 
         this.labelDirection.AutoSize = true;
         this.labelDirection.Location = new System.Drawing.Point(15, 20);
         this.labelDirection.Name = "labelDirection";
         this.labelDirection.Size = new System.Drawing.Size(91, 13);
         this.labelDirection.TabIndex = 0;
         this.labelDirection.Text = "方向 Direction:";
         // 
         // groupBoxInputs
         // 
         this.groupBoxInputs.Controls.Add(this.checkBoxRUN);
         this.groupBoxInputs.Controls.Add(this.checkBoxIDLE);
         this.groupBoxInputs.Controls.Add(this.checkBoxALARM);
         this.groupBoxInputs.Controls.Add(this.checkBoxIN_LINE);
         this.groupBoxInputs.Controls.Add(this.checkBoxEQ_ONLINE);
         this.groupBoxInputs.Controls.Add(this.checkBoxCarrier);
         this.groupBoxInputs.Controls.Add(this.checkBoxUC_REQ);
         this.groupBoxInputs.Controls.Add(this.checkBoxLC_REQ);
         this.groupBoxInputs.Controls.Add(this.checkBoxREADY);
         this.groupBoxInputs.Controls.Add(this.checkBoxU_REQ);
         this.groupBoxInputs.Controls.Add(this.checkBoxL_REQ);
         this.groupBoxInputs.Location = new System.Drawing.Point(12, 168);
         this.groupBoxInputs.Name = "groupBoxInputs";
         this.groupBoxInputs.Size = new System.Drawing.Size(300, 320);
         this.groupBoxInputs.TabIndex = 1;
         this.groupBoxInputs.TabStop = false;
         this.groupBoxInputs.Text = "EQ端輸入 (Passive->Active)";
         // 
         // checkBoxRUN
         // 
         this.checkBoxRUN.AutoSize = true;
         this.checkBoxRUN.Location = new System.Drawing.Point(18, 280);
         this.checkBoxRUN.Name = "checkBoxRUN";
         this.checkBoxRUN.Size = new System.Drawing.Size(140, 17);
         this.checkBoxRUN.TabIndex = 10;
         this.checkBoxRUN.Text = "RUN (EQ運轉中)";
         this.checkBoxRUN.UseVisualStyleBackColor = true;
         this.checkBoxRUN.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxIDLE
         // 
         this.checkBoxIDLE.AutoSize = true;
         this.checkBoxIDLE.Location = new System.Drawing.Point(18, 257);
         this.checkBoxIDLE.Name = "checkBoxIDLE";
         this.checkBoxIDLE.Size = new System.Drawing.Size(145, 17);
         this.checkBoxIDLE.TabIndex = 9;
         this.checkBoxIDLE.Text = "IDLE (EQ無工件閒置)";
         this.checkBoxIDLE.UseVisualStyleBackColor = true;
         this.checkBoxIDLE.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxALARM
         // 
         this.checkBoxALARM.AutoSize = true;
         this.checkBoxALARM.Location = new System.Drawing.Point(18, 234);
         this.checkBoxALARM.Name = "checkBoxALARM";
         this.checkBoxALARM.Size = new System.Drawing.Size(125, 17);
         this.checkBoxALARM.TabIndex = 8;
         this.checkBoxALARM.Text = "ALARM (EQ異常)";
         this.checkBoxALARM.UseVisualStyleBackColor = true;
         this.checkBoxALARM.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxIN_LINE
         // 
         this.checkBoxIN_LINE.AutoSize = true;
         this.checkBoxIN_LINE.Location = new System.Drawing.Point(18, 211);
         this.checkBoxIN_LINE.Name = "checkBoxIN_LINE";
         this.checkBoxIN_LINE.Size = new System.Drawing.Size(145, 17);
         this.checkBoxIN_LINE.TabIndex = 7;
         this.checkBoxIN_LINE.Text = "IN_LINE (EQ併入產線)";
         this.checkBoxIN_LINE.UseVisualStyleBackColor = true;
         this.checkBoxIN_LINE.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxEQ_ONLINE
         // 
         this.checkBoxEQ_ONLINE.AutoSize = true;
         this.checkBoxEQ_ONLINE.Checked = true;
         this.checkBoxEQ_ONLINE.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBoxEQ_ONLINE.Location = new System.Drawing.Point(18, 188);
         this.checkBoxEQ_ONLINE.Name = "checkBoxEQ_ONLINE";
         this.checkBoxEQ_ONLINE.Size = new System.Drawing.Size(155, 17);
         this.checkBoxEQ_ONLINE.TabIndex = 6;
         this.checkBoxEQ_ONLINE.Text = "EQ_ONLINE (EQ在線)";
         this.checkBoxEQ_ONLINE.UseVisualStyleBackColor = true;
         this.checkBoxEQ_ONLINE.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxCarrier
         // 
         this.checkBoxCarrier.AutoSize = true;
         this.checkBoxCarrier.Location = new System.Drawing.Point(18, 165);
         this.checkBoxCarrier.Name = "checkBoxCarrier";
         this.checkBoxCarrier.Size = new System.Drawing.Size(135, 17);
         this.checkBoxCarrier.TabIndex = 5;
         this.checkBoxCarrier.Text = "Carrier (工件在席)";
         this.checkBoxCarrier.UseVisualStyleBackColor = true;
         this.checkBoxCarrier.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxUC_REQ
         // 
         this.checkBoxUC_REQ.AutoSize = true;
         this.checkBoxUC_REQ.Location = new System.Drawing.Point(18, 142);
         this.checkBoxUC_REQ.Name = "checkBoxUC_REQ";
         this.checkBoxUC_REQ.Size = new System.Drawing.Size(185, 17);
         this.checkBoxUC_REQ.TabIndex = 4;
         this.checkBoxUC_REQ.Text = "UC_REQ (可載出工件請求)";
         this.checkBoxUC_REQ.UseVisualStyleBackColor = true;
         this.checkBoxUC_REQ.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxLC_REQ
         // 
         this.checkBoxLC_REQ.AutoSize = true;
         this.checkBoxLC_REQ.Location = new System.Drawing.Point(18, 119);
         this.checkBoxLC_REQ.Name = "checkBoxLC_REQ";
         this.checkBoxLC_REQ.Size = new System.Drawing.Size(185, 17);
         this.checkBoxLC_REQ.TabIndex = 3;
         this.checkBoxLC_REQ.Text = "LC_REQ (可載入工件請求)";
         this.checkBoxLC_REQ.UseVisualStyleBackColor = true;
         this.checkBoxLC_REQ.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxREADY
         // 
         this.checkBoxREADY.AutoSize = true;
         this.checkBoxREADY.Location = new System.Drawing.Point(18, 96);
         this.checkBoxREADY.Name = "checkBoxREADY";
         this.checkBoxREADY.Size = new System.Drawing.Size(155, 17);
         this.checkBoxREADY.TabIndex = 2;
         this.checkBoxREADY.Text = "READY (EQ準備完成)";
         this.checkBoxREADY.UseVisualStyleBackColor = true;
         this.checkBoxREADY.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxU_REQ
         // 
         this.checkBoxU_REQ.AutoSize = true;
         this.checkBoxU_REQ.Location = new System.Drawing.Point(18, 73);
         this.checkBoxU_REQ.Name = "checkBoxU_REQ";
         this.checkBoxU_REQ.Size = new System.Drawing.Size(165, 17);
         this.checkBoxU_REQ.TabIndex = 1;
         this.checkBoxU_REQ.Text = "U_REQ (載出工件要求)";
         this.checkBoxU_REQ.UseVisualStyleBackColor = true;
         this.checkBoxU_REQ.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // checkBoxL_REQ
         // 
         this.checkBoxL_REQ.AutoSize = true;
         this.checkBoxL_REQ.Location = new System.Drawing.Point(18, 50);
         this.checkBoxL_REQ.Name = "checkBoxL_REQ";
         this.checkBoxL_REQ.Size = new System.Drawing.Size(165, 17);
         this.checkBoxL_REQ.TabIndex = 0;
         this.checkBoxL_REQ.Text = "L_REQ (載入工件要求)";
         this.checkBoxL_REQ.UseVisualStyleBackColor = true;
         this.checkBoxL_REQ.CheckedChanged += new System.EventHandler(this.checkBoxInput_CheckedChanged);
         // 
         // groupBoxOutputs
         // 
         this.groupBoxOutputs.Controls.Add(this.labelCOMP);
         this.groupBoxOutputs.Controls.Add(this.labelBUSY);
         this.groupBoxOutputs.Controls.Add(this.labelTR_REQ);
         this.groupBoxOutputs.Controls.Add(this.labelVALID);
         this.groupBoxOutputs.Location = new System.Drawing.Point(318, 12);
         this.groupBoxOutputs.Name = "groupBoxOutputs";
         this.groupBoxOutputs.Size = new System.Drawing.Size(200, 180);
         this.groupBoxOutputs.TabIndex = 2;
         this.groupBoxOutputs.TabStop = false;
         this.groupBoxOutputs.Text = "RGV輸出 (Active->Passive)";
         // 
         // labelCOMP
         // 
         this.labelCOMP.BackColor = System.Drawing.Color.LightGray;
         this.labelCOMP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.labelCOMP.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.labelCOMP.Location = new System.Drawing.Point(15, 125);
         this.labelCOMP.Name = "labelCOMP";
         this.labelCOMP.Size = new System.Drawing.Size(170, 30);
         this.labelCOMP.TabIndex = 3;
         this.labelCOMP.Text = "COMP (完成)";
         this.labelCOMP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // labelBUSY
         // 
         this.labelBUSY.BackColor = System.Drawing.Color.LightGray;
         this.labelBUSY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.labelBUSY.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.labelBUSY.Location = new System.Drawing.Point(15, 90);
         this.labelBUSY.Name = "labelBUSY";
         this.labelBUSY.Size = new System.Drawing.Size(170, 30);
         this.labelBUSY.TabIndex = 2;
         this.labelBUSY.Text = "BUSY (忙碌)";
         this.labelBUSY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // labelTR_REQ
         // 
         this.labelTR_REQ.BackColor = System.Drawing.Color.LightGray;
         this.labelTR_REQ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.labelTR_REQ.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.labelTR_REQ.Location = new System.Drawing.Point(15, 55);
         this.labelTR_REQ.Name = "labelTR_REQ";
         this.labelTR_REQ.Size = new System.Drawing.Size(170, 30);
         this.labelTR_REQ.TabIndex = 1;
         this.labelTR_REQ.Text = "TR_REQ (轉運請求)";
         this.labelTR_REQ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // labelVALID
         // 
         this.labelVALID.BackColor = System.Drawing.Color.LightGray;
         this.labelVALID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.labelVALID.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.labelVALID.Location = new System.Drawing.Point(15, 20);
         this.labelVALID.Name = "labelVALID";
         this.labelVALID.Size = new System.Drawing.Size(170, 30);
         this.labelVALID.TabIndex = 0;
         this.labelVALID.Text = "VALID (有效)";
         this.labelVALID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // groupBoxLog
         // 
         this.groupBoxLog.Controls.Add(this.textBoxLog);
         this.groupBoxLog.Location = new System.Drawing.Point(524, 12);
         this.groupBoxLog.Name = "groupBoxLog";
         this.groupBoxLog.Size = new System.Drawing.Size(450, 476);
         this.groupBoxLog.TabIndex = 3;
         this.groupBoxLog.TabStop = false;
         this.groupBoxLog.Text = "事件記錄 Event Log";
         // 
         // textBoxLog
         // 
         this.textBoxLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBoxLog.Location = new System.Drawing.Point(10, 20);
         this.textBoxLog.Multiline = true;
         this.textBoxLog.Name = "textBoxLog";
         this.textBoxLog.ReadOnly = true;
         this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
         this.textBoxLog.Size = new System.Drawing.Size(430, 445);
         this.textBoxLog.TabIndex = 0;
         // 
         // timerUpdate
         // 
         this.timerUpdate.Interval = 100;
         this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
         // 
         // groupBoxStatus
         // 
         this.groupBoxStatus.Controls.Add(this.labelStatusInfo);
         this.groupBoxStatus.Location = new System.Drawing.Point(318, 198);
         this.groupBoxStatus.Name = "groupBoxStatus";
         this.groupBoxStatus.Size = new System.Drawing.Size(200, 290);
         this.groupBoxStatus.TabIndex = 4;
         this.groupBoxStatus.TabStop = false;
         this.groupBoxStatus.Text = "狀態資訊 Status";
         // 
         // labelStatusInfo
         // 
         this.labelStatusInfo.AutoSize = true;
         this.labelStatusInfo.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.labelStatusInfo.Location = new System.Drawing.Point(15, 25);
         this.labelStatusInfo.Name = "labelStatusInfo";
         this.labelStatusInfo.Size = new System.Drawing.Size(49, 14);
         this.labelStatusInfo.TabIndex = 0;
         this.labelStatusInfo.Text = "Ready";
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(984, 501);
         this.Controls.Add(this.groupBoxStatus);
         this.Controls.Add(this.groupBoxLog);
         this.Controls.Add(this.groupBoxOutputs);
         this.Controls.Add(this.groupBoxInputs);
         this.Controls.Add(this.groupBoxController);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.MaximizeBox = false;
         this.Name = "Form1";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "E84 RGV/AGV Controller Test (Active Side)";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
         this.Load += new System.EventHandler(this.Form1_Load);
         this.groupBoxController.ResumeLayout(false);
         this.groupBoxController.PerformLayout();
         this.groupBoxInputs.ResumeLayout(false);
         this.groupBoxInputs.PerformLayout();
         this.groupBoxOutputs.ResumeLayout(false);
         this.groupBoxLog.ResumeLayout(false);
         this.groupBoxLog.PerformLayout();
         this.groupBoxStatus.ResumeLayout(false);
         this.groupBoxStatus.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.GroupBox groupBoxController;
      private System.Windows.Forms.Label labelDirection;
      private System.Windows.Forms.RadioButton radioLoad;
      private System.Windows.Forms.RadioButton radioUnload;
      private System.Windows.Forms.Button buttonStart;
      private System.Windows.Forms.Button buttonStop;
      private System.Windows.Forms.Label labelState;
      private System.Windows.Forms.Label labelCurrentState;
      private System.Windows.Forms.GroupBox groupBoxInputs;
      private System.Windows.Forms.CheckBox checkBoxL_REQ;
      private System.Windows.Forms.CheckBox checkBoxU_REQ;
      private System.Windows.Forms.CheckBox checkBoxREADY;
      private System.Windows.Forms.CheckBox checkBoxLC_REQ;
      private System.Windows.Forms.CheckBox checkBoxUC_REQ;
      private System.Windows.Forms.CheckBox checkBoxCarrier;
      private System.Windows.Forms.CheckBox checkBoxEQ_ONLINE;
      private System.Windows.Forms.CheckBox checkBoxIN_LINE;
      private System.Windows.Forms.CheckBox checkBoxALARM;
      private System.Windows.Forms.CheckBox checkBoxIDLE;
      private System.Windows.Forms.CheckBox checkBoxRUN;
      private System.Windows.Forms.GroupBox groupBoxOutputs;
      private System.Windows.Forms.Label labelVALID;
      private System.Windows.Forms.Label labelTR_REQ;
      private System.Windows.Forms.Label labelBUSY;
      private System.Windows.Forms.Label labelCOMP;
      private System.Windows.Forms.GroupBox groupBoxLog;
      private System.Windows.Forms.TextBox textBoxLog;
      private System.Windows.Forms.Timer timerUpdate;
      private System.Windows.Forms.GroupBox groupBoxStatus;
      private System.Windows.Forms.Label labelStatusInfo;
   }
}
