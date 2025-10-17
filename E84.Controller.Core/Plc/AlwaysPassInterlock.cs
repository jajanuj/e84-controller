using E84.Controller.Core.Interfaces;
using E84.Controller.Core.Models;

namespace E84.Controller.Core.Plc
{
   /// <summary>
   /// 測試用的安全互鎖實作，永遠回傳通過（用於測試環境）。
   /// </summary>
   public class AlwaysPassInterlock : IE84SafetyInterlock
   {
      public bool Check(E84State state, out string reason)
      {
         reason = string.Empty;
         return true;
      }
   }
}
