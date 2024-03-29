﻿namespace NodeHostEnvironment.NativeHost
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using InProcess;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EntryPointSignature(
        IntPtr context,
        IntPtr nativeMethods,
        int argc,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)]
        string[] argv,
        IntPtr result);

    internal static class NativeEntryPoint
    {
        private static readonly EntryPointSignature CompileCheck = RunHostedApplication;

        internal static void RunHostedApplication(IntPtr context,
                                                  IntPtr nativeMethodsPtr,
                                                  int argc,
                                                  [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)]
                                                  string[] argv,
                                                  IntPtr resultValue)
        {
            // Switch to default ALC
            var myAlc = AssemblyLoadContext.GetLoadContext(typeof(NativeEntryPoint).Assembly);
            if (myAlc != AssemblyLoadContext.Default)
            {
                var inCtx = AssemblyLoadContext.Default.LoadFromAssemblyName(typeof(NativeEntryPoint).Assembly.GetName());
                var tInCtx = inCtx.GetType(typeof(NativeEntryPoint).FullName);
                tInCtx.GetMethod(nameof(RunHostedApplication), BindingFlags.Static | BindingFlags.NonPublic)
                      .Invoke(null, new object[] { context, nativeMethodsPtr, argc, argv, resultValue });
                return;
            }

            var nativeMethods = Marshal.PtrToStructure<NativeApi>(nativeMethodsPtr);

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
                                                                                 new object[] { argv.Skip(1).ToArray() }),
                                                           null);

                Marshal.StructureToPtr(DotNetValue.FromObject(result, host), resultValue, false);
            }            
            catch (Exception e)
            {
                Marshal.StructureToPtr(DotNetValue.FromObject(UnwrapExceptions(e), host), resultValue, false);
            }
        }

        private static Exception UnwrapExceptions(Exception exception)
        {
            switch (exception)
            {
                case AggregateException ae:
                    return UnwrapExceptions(UnwrapAggregateException(ae));
                case TargetInvocationException tie:
                    return UnwrapExceptions(tie.InnerException);
                default:
                    return exception;
            }
        }

        private static Exception UnwrapAggregateException(AggregateException exception)
        {
            if (null == exception)
                return null;
            exception = exception.Flatten();
            if (exception.InnerExceptions.Count == 1)            
                return exception.InnerExceptions[0];
            return exception;
        }
    }
}
