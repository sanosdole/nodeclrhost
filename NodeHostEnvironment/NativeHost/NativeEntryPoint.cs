using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NodeHostEnvironment.NativeHost
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int EntryPointSignature(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 0)] string[] argv);

    public static class NativeEntryPoint
    {

        public static int RunHostedApplication(int argc, string[] argv)
        {
            var assembly_path = argv[0];
            var assembly = Assembly.LoadFile(assembly_path);
            var entryPoint = assembly.EntryPoint;
            return (int) entryPoint.Invoke(null, new object[] { argv });
        }
    }
}