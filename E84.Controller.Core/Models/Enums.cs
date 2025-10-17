namespace E84.Controller.Core.Models
{
   /// <summary>
   /// 轉運方向。
   /// </summary>
   public enum E84Direction
   {
      Inbound,
      Outbound
   }

   /// <summary>
   /// E84 主要狀態（簡化版，涵蓋握手流程）。
   /// </summary>
   public enum E84State
   {
      /// <summary>Idle：無活動的轉運。</summary>
      Idle,

      /// <summary>Request：偵測到請求（TR_REQ 或 L/U_REQ）。</summary>
      Request,

      /// <summary>Busy：我方已宣告 BUSY/HOLD/CLAMP，等待 VALID 或下一步。</summary>
      Busy,

      /// <summary>Valid：接收到對端 VALID 回授。</summary>
      Valid,

      /// <summary>Transfer：宣告 TRANSFER 並等待 COMPT。</summary>
      Transfer,

      /// <summary>Complete：完成並清除輸出。</summary>
      Complete,

      /// <summary>Abort：中止流程並宣告 ABORT。</summary>
      Abort,

      /// <summary>Error：不可恢復的錯誤狀態（如 PLC 異常/逾時）。</summary>
      Error,

      /// <summary>Resetting：中止後等待 RESET 條件再回到 Idle。</summary>
      Resetting
   }

   public enum E84Event
   {
      StateEntered,
      StateLeft,
      SignalRising,
      SignalFalling,
      Timeout,
      PlcDisconnected,
      PlcReconnected,
      InterlockFailed,
      AbortIssued
   }

   public enum E84FaultCode
   {
      None,
      PlcDisconnected,
      TimeoutWaitTrReq,
      TimeoutWaitValid,
      TimeoutWaitCompt,
      InterlockFailed,
      OutputWriteFailed,
      Unknown
   }
}