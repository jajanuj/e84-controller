namespace E84.Controller.Core.Models
{
   /// <summary>
   /// 描述 PLC 裝置映射及反向屬性。
   /// </summary>
   public struct PlcDevice
   {
      /// <summary>
      /// 裝置位址，例如 "X100", "Y201", "M300", "D400"。
      /// </summary>
      public string Address { get; set; }

      /// <summary>
      /// 若為 true，表示邏輯有效（Active）與 PLC 實際位元極性相反（Active Low）。
      /// </summary>
      public bool Inverted { get; set; }

      public override string ToString() => $"{Address}{(Inverted ? "(inv)" : "")}";
   }
}