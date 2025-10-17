using System;
using System.Threading;
using E84.Controller.Core.Exceptions;
using E84.Controller.Core.Interfaces;

namespace E84.Controller.Core.Plc
{
   /// <summary>
   /// 以 Mitsubishi MX Component (ActUtlType / ActProgType) 實作的 IE84Plc 範本。
   /// 注意：此檔為模板/範例，須在專案中加入 ActUtlType COM 參考並用實際呼叫取代未實作部分，
   /// 並將 MX 回傳碼轉為 PlcException。
   /// </summary>
   public class MxComponentPlc : IE84Plc
   {
      #region Fields

      private readonly string _host;
      private readonly int _port;
      private bool _connected = false;

      #endregion

      #region Constructors

      /// <summary>
      /// 以連線參數（例如 IP 或站號）建構。
      /// </summary>
      public MxComponentPlc(string hostOrStation, int port = 0)
      {
         _host = hostOrStation;
         _port = port;
      }

      #endregion

      public bool IsConnected => _connected;

      public void Open()
      {
         // TODO: replace with actual ActUtlType call e.g.
         // var act = new ActUtlType();
         // int ret = act.Open();
         // if (ret != 0) throw new PlcException($"MXComponent Open failed: {ret}");
         // _act = act;
         // _connected = true;
         // For now mark connected for template.
         _connected = true;
      }

      public void Close()
      {
         // TODO: Call act.Close() and release COM object
         _connected = false;
      }

      public bool ReadBit(string device)
      {
         if (!_connected) throw new PlcException("Not connected", device);
         // TODO: call act.GetDevice(device, ref shortVal)
         // translate return codes
         throw new NotImplementedException("Implement MX Component ReadBit");
      }

      public int ReadWord(string device)
      {
         if (!_connected) throw new PlcException("Not connected", device);
         throw new NotImplementedException("Implement MX Component ReadWord");
      }

      public void WriteBit(string device, bool value)
      {
         if (!_connected) throw new PlcException("Not connected", device);
         throw new NotImplementedException("Implement MX Component WriteBit");
      }

      public void WriteWord(string device, int value)
      {
         if (!_connected) throw new PlcException("Not connected", device);
         throw new NotImplementedException("Implement MX Component WriteWord");
      }

      public void EnsureConnected(int retryCount = 3, int retryDelayMs = 1000)
      {
         int attempts = 0;
         while (attempts++ < retryCount)
         {
            try
            {
               Open();
               if (IsConnected) return;
            }
            catch
            {
               // ignore and retry
            }

            Thread.Sleep(retryDelayMs);
         }

         throw new PlcException("Failed to ensure connection");
      }

      public void Dispose()
      {
         Close();
      }
   }
}