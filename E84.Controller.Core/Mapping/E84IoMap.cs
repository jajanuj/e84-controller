using System;
using System.Collections.Generic;
using E84.Controller.Core.Interfaces;
using E84.Controller.Core.Models;

namespace E84.Controller.Core.Mapping
{
   /// <summary>
   /// 透過字典注入建構的 IE84IoMap 基本實作。
   /// 支援多站位及單一信號的反向設定。
   /// </summary>
   public class E84IoMap : IE84IoMap
   {
      #region Constructors

      /// <summary>
      /// 建構映射表。
      /// </summary>
      /// <param name="inputs">邏輯輸入名稱 -> 裝置 (Address, Inverted)</param>
      /// <param name="outputs">邏輯輸出名稱 -> 裝置</param>
      /// <param name="debounceMs">輸入去抖時間（毫秒）</param>
      public E84IoMap(IDictionary<string, PlcDevice> inputs,
         IDictionary<string, PlcDevice> outputs,
         int debounceMs = 10)
      {
         Inputs = new Dictionary<string, PlcDevice>(inputs ?? new Dictionary<string, PlcDevice>(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
         Outputs = new Dictionary<string, PlcDevice>(outputs ?? new Dictionary<string, PlcDevice>(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
         DebounceMs = Math.Max(0, debounceMs);
      }

      #endregion

      public IReadOnlyDictionary<string, PlcDevice> Inputs { get; }
      public IReadOnlyDictionary<string, PlcDevice> Outputs { get; }
      public int DebounceMs { get; }

      /// <summary>
      /// 取得輸入映射（若不存在回傳 null）。
      /// </summary>
      public PlcDevice? GetInput(string signalName)
      {
         if (signalName == null) return null;
         if (Inputs.TryGetValue(signalName, out var d)) return d;
         return null;
      }

      /// <summary>
      /// 取得輸出映射（若不存在回傳 null）。
      /// </summary>
      public PlcDevice? GetOutput(string signalName)
      {
         if (signalName == null) return null;
         if (Outputs.TryGetValue(signalName, out var d)) return d;
         return null;
      }
   }
}