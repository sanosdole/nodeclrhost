using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NodeHostEnvironment.InProcess;

namespace NodeHostEnvironment.NativeHost
{

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate DotNetValue EntryPointSignature(
        IntPtr context,
        // TODO DM 20.03.2020: Pass API as struct with delegates
        int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 1)] string[] argv);

    internal static class NativeEntryPoint
    {
        private static readonly EntryPointSignature CompileCheck = RunHostedApplication;

        internal static DotNetValue RunHostedApplication(IntPtr context,
            // TODO DM 20.03.2020: Pass API as struct with delegates
            int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)] string[] argv)
        {
            var pathToCoreClrHostingModule = Environment.GetEnvironmentVariable("CORECLR_HOSTING_MODULE_PATH") ??
                "./node_modules/coreclr-hosting/build/Release/coreclr-hosting.node";
            var nativeMethods = DynamicLibraryLoader.LoadApi<DelegateBasedNativeApi>(pathToCoreClrHostingModule);
            var host = NativeHost.Host = new NativeNodeHost(context, nativeMethods);

            try
            {
                var assembly_path = argv[0];
                var assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assembly_path));
                var entryPoint = assembly.EntryPoint;
                var result = entryPoint.Invoke(null, new object[] { argv.Skip(1).ToArray() });
                return DotNetValue.FromObject(result ?? 0, host);
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