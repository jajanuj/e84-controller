using System;
using System.Collections.Concurrent;
using System.Threading;
using E84.Controller.Core.Exceptions;
using E84.Controller.Core.Interfaces;

namespace E84.Controller.Core.Plc
{
   /// <summary>
   /// 測試用的 PLC 替身 (Fake)，實作 IE84Plc，可模擬輸入變化並觀察輸出。
   /// 可模擬短暫或永久斷線以驗證重連邏輯。
   /// </summary>
   public class FakePlc : IE84Plc
   {
      #region Fields

      private ConcurrentDictionary<string, bool> _bits = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
      private volatile bool _connected = true;

      #endregion

      #region Public Methods

      /// <summary>
      /// 測試用：模擬 PLC 輸入變更。
      /// </summary>
      public void SetInput(string device, bool value) => _bits[device] = value;

      /// <summary>
      /// 測試用：模擬連線斷開或回復。
      /// </summary>
      public void SimulateDisconnect(bool disconnect)
      {
         _connected = !disconnect ? true : false;
      }

      #endregion

      public bool IsConnected => _connected;

      public void Open() => _connected = true;

      public void Close() => _connected = false;

      public void Dispose() => Close();

      public bool ReadBit(string device)
      {
         if (!_connected) throw new PlcException("PLC not connected", device);
         if (_bits.TryGetValue(device, out bool v)) return v;
         return false;
      }

      public int ReadWord(string device)
      {
         if (!_connected) throw new PlcException("PLC not connected", device);
         return 0;
      }

      public void WriteBit(string device, bool value)
      {
         if (!_connected) throw new PlcException("PLC not connected", device);
         _bits[device] = value;
      }

      public void WriteWord(string device, int value)
      {
         if (!_connected) throw new PlcException("PLC not connected", device);
      }

      public void EnsureConnected(int retryCount = 3, int retryDelayMs = 1000)
      {
         int tries = 0;
         while (!_connected && tries++ < retryCount)
         {
            Thread.Sleep(retryDelayMs);
         }

         if (!_connected) throw new PlcException("Could not connect");
      }
   }
}