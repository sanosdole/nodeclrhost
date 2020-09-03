namespace NodeHostEnvironment
{
   /// <summary>
   /// Singleton to retrieve the <see cref="IBridgeToNode"/> <see cref="Instance"/> in node hosted applications.
   /// </summary>
   public static class NodeHost
   {
      /// <summary>
      /// The bridge instance for this hosted application.
      /// </summary>
      public static IBridgeToNode Instance { get; internal set; }
   }
}
