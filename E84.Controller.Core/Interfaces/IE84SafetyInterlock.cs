using E84.Controller.Core.Models;

namespace E84.Controller.Core.Interfaces
{
   /// <summary>
   /// 檢查安全條件（門閉合、急停、真空、夾爪到位等）。
   /// 實作必須快速且無副作用（不應改變機台狀態）。
   /// </summary>
   public interface IE84SafetyInterlock
   {
      #region Public Methods

      /// <summary>
      /// 根據給定狀態檢查安全條件，若安全回傳 true；否則回傳 false 並提供簡短原因。
      /// </summary>
      bool Check(E84State state, out string reason);

      #endregion
   }
}