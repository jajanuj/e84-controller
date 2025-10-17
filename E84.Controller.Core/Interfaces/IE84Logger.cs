using System;

namespace E84.Controller.Core.Interfaces
{
   /// <summary>
   /// 控制器使用的簡易日誌介面。實作者可將其包裝為 Microsoft.Extensions.Logging.ILogger。
   /// 日誌條目需包含毫秒解析度的時間戳（由實作者負責）。
   /// </summary>
   public interface IE84Logger
   {
      #region Public Methods

      void Info(string message);
      void Warn(string message);
      void Error(string message, Exception ex = null);
      void Debug(string message);

      #endregion
   }
}