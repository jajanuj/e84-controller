using System;

namespace E84.Controller.Core.Interfaces
{
   /// <summary>
   /// E84 控制器使用的 PLC 讀寫抽象介面。
   /// 實作應該包裝 MX Component (ActUtlType/ActProgType) 或提供測試替身 (Fake)。
   /// 讀寫失敗應拋出 PlcException。
   /// </summary>
   public interface IE84Plc : IDisposable
   {
      #region Properties

      /// <summary>
      /// 是否已與 PLC 連線。
      /// </summary>
      bool IsConnected { get; }

      #endregion

      #region Public Methods

      /// <summary>
      /// 開啟與 PLC 的連線。若已開啟應是冪等操作。
      /// </summary>
      void Open();

      /// <summary>
      /// 關閉 PLC 連線。
      /// </summary>
      void Close();

      /// <summary>
      /// 讀取單一位元裝置（例如 "X100", "M300"）。
      /// 回傳 true 表示該位元為 1（反向處理應由映射負責）。
      /// </summary>
      bool ReadBit(string device);

      /// <summary>
      /// 寫入單一位元裝置（例如 "Y200","M300"）。
      /// </summary>
      void WriteBit(string device, bool value);

      /// <summary>
      /// 選用：讀取字（例如 D 區）。若未實作可丟 NotSupportedException。
      /// </summary>
      int ReadWord(string device);

      /// <summary>
      /// 選用：寫入字（例如 D 區）。
      /// </summary>
      void WriteWord(string device, int value);

      /// <summary>
      /// 以給定重試次數與延遲確保連線。實作應在成功或放棄前阻塞並在失敗時拋 PlcException。
      /// </summary>
      void EnsureConnected(int retryCount = 3, int retryDelayMs = 1000);

      #endregion
   }
}