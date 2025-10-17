using System;
using System.Collections.Generic;

namespace E84.Controller.Core.Models
{
   /// <summary>
   /// 由 GetStatus 回傳之狀態快照模型。
   /// </summary>
   public class E84StatusSnapshot
   {
      #region Properties

      public E84State State { get; set; }
      public E84Direction Direction { get; set; }
      public Dictionary<string, bool> Inputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
      public Dictionary<string, bool> Outputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
      public string LastError { get; set; }
      public DateTime TimestampUtc { get; set; }

      #endregion
   }
}