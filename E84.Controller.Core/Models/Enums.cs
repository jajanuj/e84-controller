namespace E84.Controller.Core.Models
{
   /// <summary>
   /// 轉運方向。
   /// </summary>
   public enum E84Direction
   {
      /// <summary>Load (入料) - RGV載入工件到EQ</summary>
      Load,
      /// <summary>Unload (出料) - RGV從EQ載出工件</summary>
      Unload
   }

   /// <summary>
   /// E84 主要狀態 - Active側(RGV/AGV)狀態機。
   /// </summary>
   public enum E84State
   {
      /// <summary>Idle：等待來自LCS的搬送命令或EQ端的請求。</summary>
      Idle,

      /// <summary>WaitingRequest：RGV到達EQ位置，已發送VALID，等待EQ的L_REQ/U_REQ。</summary>
      WaitingRequest,

      /// <summary>TrReqSent：已發送TR_REQ，等待EQ的READY。</summary>
      TrReqSent,

      /// <summary>ReadyReceived：收到READY，準備發送BUSY並開始搬運。</summary>
      ReadyReceived,

      /// <summary>Transferring：BUSY ON，正在進行工件搬運。</summary>
      Transferring,

      /// <summary>TransferComplete：搬運完成，離開交握區，BUSY OFF，發送COMP。</summary>
      TransferComplete,

      /// <summary>WaitingReadyOff：COMP已發送，等待EQ的READY OFF。</summary>
      WaitingReadyOff,

      /// <summary>Complete：交握完成，準備返回Idle。</summary>
      Complete,

      /// <summary>Abort：異常中止流程。</summary>
      Abort,

      /// <summary>Error：不可恢復的錯誤狀態。</summary>
      Error,

      /// <summary>Resetting：異常後復原中。</summary>
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
      /// <summary>T1: 發出VALID後等待L_REQ/U_REQ超時</summary>
      TimeoutT1_WaitLReqUReq,
      /// <summary>T3: 發出TR_REQ後等待READY超時</summary>
      TimeoutT3_WaitReady,
      /// <summary>T5: 發出BUSY後等待COMP超時</summary>
      TimeoutT5_WaitComp,
      /// <summary>T6: 發出COMP後等待READY OFF超時</summary>
      TimeoutT6_WaitReadyOff,
      /// <summary>L_REQ和U_REQ同時ON</summary>
      ErrorBothLReqUReqOn,
      /// <summary>TR_REQ未ON，READY先ON</summary>
      ErrorReadyBeforeTrReq,
      /// <summary>BUSY ON時，READY OFF</summary>
      ErrorReadyOffDuringBusy,
      /// <summary>移動到EQ Port時，LC_REQ或UC_REQ OFF</summary>
      ErrorLcUcReqOff,
      InterlockFailed,
      OutputWriteFailed,
      Unknown
   }
}