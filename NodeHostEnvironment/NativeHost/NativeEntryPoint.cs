namespace NodeHostEnvironment.NativeHost
{
   using System;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Reflection;
   using System.Runtime.InteropServices;
   using System.Runtime.Loader;
   using System.Threading;
   using System.Threading.Tasks;
   using InProcess;

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void EntryPointSignature(
      IntPtr context,
      IntPtr nativeMethods,
      int argc,
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)]
      string[] argv,
      IntPtr result);

   // ReSharper disable once UnusedType.Global as it is called from native code
   internal static class NativeEntryPoint
   {
      // ReSharper disable once UnusedMember.Local as it is only used as a compile check
      private static readonly EntryPointSignature CompileCheck = RunHostedApplication;

      internal static void RunHostedApplication(IntPtr context,
                                                IntPtr nativeMethodsPtr,
                                                int argc,
                                                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)]
                                                string[] argv,
                                                IntPtr resultValue)
      {
         // Switch to default AssemblyLoadContext if not already in default context
         var myAssembly = typeof(NativeEntryPoint).Assembly;
         var myAssemblyLoadContext = AssemblyLoadContext.GetLoadContext(myAssembly);
         if (myAssemblyLoadContext != AssemblyLoadContext.Default)
         {
            var myAssemblyInDefaultContext = AssemblyLoadContext.Default.LoadFromAssemblyName(myAssembly.GetName());
            var fullName = typeof(NativeEntryPoint).FullName;
            Debug.Assert(fullName != null);
            var myTypeInDefaultContext = myAssemblyInDefaultContext.GetType(fullName);
            Debug.Assert(myTypeInDefaultContext != null);
            var thisMethodInDefaultContext = myTypeInDefaultContext.GetMethod(nameof(RunHostedApplication),
                                                                              BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(thisMethodInDefaultContext != null);
            thisMethodInDefaultContext.Invoke(null,
                                              new object[]
                                              {
                                                 context,
                                                 nativeMethodsPtr,
                                                 argc,
                                                 argv,
                                                 resultValue
                                              });
            return;
         }

         var nativeMethods = Marshal.PtrToStructure<NativeApi>(nativeMethodsPtr);

         var host = new NativeNodeHost(context, nativeMethods);
         NodeHost.Instance = new NodeBridge(host);
         // TODO DM 03.09.2020: Find out why this does not get called!
         //// DM 02.09.2020: We dispose when the ALC is unloading, so that we clean up the native context at the latest possible moment.
         //myAssemblyLoadContext.Unloading += _ =>
         //                                   {
         //                                      NodeHost.Instance = null;
         //                                      host.Dispose();
         //                                   };
         try
         {
            var assemblyPath = argv[0];
            var assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyPath));

            var entryPoint = assembly.EntryPoint;
            if (entryPoint == null)
               throw new InvalidOperationException($"Assembly '{assemblyPath}' has no entry point!");

            if (entryPoint.DeclaringType == null)
               throw new InvalidOperationException($"EntryPoint '{entryPoint.Name}' from '{assemblyPath}' has no declaring type!");

            if (entryPoint.IsSpecialName && entryPoint.Name.StartsWith("<") && entryPoint.Name.EndsWith(">"))
               entryPoint = entryPoint.DeclaringType.GetMethod(entryPoint.Name.Substring(1, entryPoint.Name.Length - 2),
                                                               BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            var result = host.Scheduler
                             .RunCallbackSynchronously(ExecuteMainMethod,
                                                       (entryPoint, argv.Skip(1).ToArray()));

            Marshal.StructureToPtr(DotNetValue.FromObject(result, host), resultValue, false);

            if (result is Task resultTask)
            {
               // TODO DM 03.09.2020: This is just a workaround! We need a proper EntryPoint interface and more control from JS.
               //                     Consider something like a static function that receives the bridge interface, a close callback & js args.
               //                     This may call a entry point or do whatever it wants.
               //                     This way we can even utilize ALCs and create a NativeContext per JS context (Although they all must share the dotnethost).
               // TODO DM 03.09.2020: When completing the task on the `unload` event in a electron renderer, this crashes occasionally
               resultTask.ContinueWith(t =>
                                       {
                                          NodeHost.Instance = null;
                                          host.Dispose();
                                       });
            }
            else
            {
               NodeHost.Instance = null;
               host.Dispose();
            }
         }
         catch (TargetInvocationException tie)
         {
            Marshal.StructureToPtr(DotNetValue.FromObject(tie.InnerException, host), resultValue, false);
         }
         catch (Exception e)
         {
            Marshal.StructureToPtr(DotNetValue.FromObject(e, host), resultValue, false);
         }
      }

      private static object ExecuteMainMethod(object state)
      {
         var (method, args) = ((MethodInfo, string[]))state;
         return method.Invoke(null,
                              new object[]
                              {
                                 args
                              });
      }
   }
}
