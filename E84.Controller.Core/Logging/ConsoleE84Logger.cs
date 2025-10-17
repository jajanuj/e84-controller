using E84.Controller.Core.Interfaces;
using System;

namespace E84.Controller.Core.Logging
{
   /// <summary>
   /// 簡單的 Console 日誌實作，用於偵錯或測試。
   /// </summary>
   public class ConsoleE84Logger : IE84Logger
   {
      #region Private Methods

      private string Timestamp() => DateTime.UtcNow.ToString("o"); // ISO with ms

      #endregion

      public void Debug(string message) => Console.WriteLine($"{Timestamp()} [DBG] {message}");
      public void Error(string message, Exception ex = null) => Console.WriteLine($"{Timestamp()} [ERR] {message} {ex}");
      public void Info(string message) => Console.WriteLine($"{Timestamp()} [INF] {message}");
      public void Warn(string message) => Console.WriteLine($"{Timestamp()} [WRN] {message}");
   }
}