using System;
using E84.Controller.Core.Models;

namespace E84.Controller.Core.Exceptions
{
   /// <summary>
   /// 包含裝置上下文的 PLC 例外封裝。
   /// </summary>
   public class PlcException : Exception
   {
      #region Constructors

      public PlcException(string message, string device = null, Exception inner = null)
         : base(message, inner)
      {
         Device = device;
      }

      #endregion

      #region Properties

      public string Device { get; }

      #endregion
   }

   /// <summary>
   /// 在狀態步驟逾時時拋出。
   /// </summary>
   public class E84TimeoutException : Exception
   {
      #region Constructors

      public E84TimeoutException(E84FaultCode code, E84State state, string signal, string message = null)
         : base(message ?? $"Timeout {code} at state {state} waiting for {signal}")
      {
         FaultCode = code;
         State = state;
         Signal = signal;
      }

      #endregion

      #region Properties

      public E84FaultCode FaultCode { get; }
      public E84State State { get; }
      public string Signal { get; }

      #endregion
   }

   /// <summary>
   /// 在安全互鎖檢查失敗時拋出。
   /// </summary>
   public class E84InterlockException : Exception
   {
      #region Constructors

      public E84InterlockException(string interlockName, string message = null)
         : base(message ?? $"Interlock failed: {interlockName}")
      {
         InterlockName = interlockName;
      }

      #endregion

      #region Properties

      public string InterlockName { get; }

      #endregion
   }
}