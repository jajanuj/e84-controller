using System.Collections.Generic;

namespace E84.Controller.Core.Models
{
   /// <summary>
   /// E84 控制器的時序與重試設定 - Active側(RGV/AGV)。
   /// </summary>
   public class E84Config
   {
      #region Properties

      public int PollIntervalMs { get; set; } = 20;
      public int DebounceMs { get; set; } = 10;

      /// <summary>
      /// 所有交握訊號的回覆延遲 (規格要求0.5秒)
      /// </summary>
      public int SignalResponseDelayMs { get; set; } = 500;

      /// <summary>
      /// T1: 發出VALID後等待L_REQ或U_REQ的超時時間 (預設5秒)
      /// </summary>
      public int T1_WaitLReqUReqMs { get; set; } = 5000;

      /// <summary>
      /// T3: 發出TR_REQ後等待READY的超時時間 (依現場，預設5秒)
      /// </summary>
      public int T3_WaitReadyMs { get; set; } = 5000;

      /// <summary>
      /// T5: 發出BUSY後等待動作完成(內部)的超時時間 (依現場，預設10秒)
      /// </summary>
      public int T5_TransferActionMs { get; set; } = 10000;

      /// <summary>
      /// T6: 發出COMP後等待READY OFF的超時時間 (預設5秒)
      /// </summary>
      public int T6_WaitReadyOffMs { get; set; } = 5000;

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