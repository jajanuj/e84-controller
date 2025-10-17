using System.Collections.Generic;

namespace E84.Controller.Core.Models
{
   /// <summary>
   /// E84 控制器的時序與重試設定。
   /// </summary>
   public class E84Config
   {
      #region Properties

      public int PollIntervalMs { get; set; } = 20;
      public int DebounceMs { get; set; } = 10;

      public int WaitTrReqMs { get; set; } = 5000;
      public int WaitValidMs { get; set; } = 5000;
      public int WaitComptMs { get; set; } = 5000;
      public int ClampMs { get; set; } = 1500;
      public int DockMs { get; set; } = 3000;

      public int PlcReconnectDelayMs { get; set; } = 1000;
      public int PlcReconnectMaxAttempts { get; set; } = 5;

      /// <summary>
      /// 要監控的信號清單；協助控制器建立 I/O 快照（選用）。
      /// </summary>
      public IList<string> MonitoredInputs { get; set; } = new List<string>();

      public IList<string> MonitoredOutputs { get; set; } = new List<string>();

      #endregion
   }
}