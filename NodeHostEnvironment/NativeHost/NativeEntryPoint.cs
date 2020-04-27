namespace NodeHostEnvironment.NativeHost
{
   using System;
   using System.IO;
   using System.Linq;
   using System.Reflection;
   using System.Runtime.InteropServices;
   using InProcess;

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate DotNetValue EntryPointSignature(
      IntPtr context,
      NativeApi nativeMethods,
      int argc,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)]
      string[] argv);

   internal static class NativeEntryPoint
   {
      private static readonly EntryPointSignature CompileCheck = RunHostedApplication;

      internal static DotNetValue RunHostedApplication(IntPtr context,
                                                       NativeApi nativeMethods,
                                                       int argc,
                                                       [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)]
                                                       string[] argv)
      {
         var host = new NativeNodeHost(context, nativeMethods);
         NodeHost.Instance = new NodeBridge(host);

         try
         {
            var assembly_path = argv[0];
            var assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assembly_path));
            var entryPoint = assembly.EntryPoint;
            if (entryPoint.IsSpecialName && entryPoint.Name.StartsWith("<") && entryPoint.Name.EndsWith(">"))
               entryPoint = entryPoint.DeclaringType.GetMethod(entryPoint.Name.Substring(1, entryPoint.Name.Length - 2),
                                                               BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            var result = host.Scheduler
                             .RunCallbackSynchronously(s =>
                                                          entryPoint.Invoke(null,
                                                                            new object[]
                                                                            {
                                                                               argv.Skip(1).ToArray()
                                                                            }),
                                                       null);

            return DotNetValue.FromObject(result, host);
         }
         catch (TargetInvocationException tie)
         {
            return DotNetValue.FromObject(tie.InnerException, host);
         }
         catch (Exception e)
         {
            return DotNetValue.FromObject(e, host);
         }
      }
   }
}
