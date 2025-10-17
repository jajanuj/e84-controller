using System.Collections.Generic;
using E84.Controller.Core.Models;

namespace E84.Controller.Core.Interfaces
{
   /// <summary>
   /// E84 訊號到 PLC 裝置位址的唯讀映射，包含反向與去抖設定。
   /// 裝置位址範例: "X100", "Y200", "M300", "D400"。
   /// </summary>
   public interface IE84IoMap
   {
      #region Properties

      /// <summary>
      /// 回傳所有輸入映射的唯讀集合。
      /// </summary>
      IReadOnlyDictionary<string, PlcDevice> Inputs { get; }

      /// <summary>
      /// 回傳所有輸出映射的唯讀集合。
      /// </summary>
      IReadOnlyDictionary<string, PlcDevice> Outputs { get; }

      /// <summary>
      /// 輸入的預設去抖時間（毫秒）。
      /// </summary>
      int DebounceMs { get; }

      #endregion

      #region Public Methods

      /// <summary>
      /// 取得輸入信號邏輯名稱對應的 PLC 裝置。若未配置則回傳 null。
      /// </summary>
      PlcDevice? GetInput(string signalName);

      /// <summary>
      /// 取得輸出信號邏輯名稱對應的 PLC 裝置。若未配置則回傳 null。
      /// </summary>
      PlcDevice? GetOutput(string signalName);

      #endregion
   }
}