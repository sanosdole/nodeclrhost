using System;
using System.IO;
using System.Linq;
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
            Console.WriteLine($"assembly_path: {assembly_path}");
            Console.WriteLine($"Entry assembly: {Assembly.GetEntryAssembly()?.FullName}");
            var assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assembly_path));
            Console.WriteLine($"Loaded assembly: {assembly?.FullName}");
            var entryPoint = assembly.EntryPoint;
            Console.WriteLine($"Entrypoint: {entryPoint?.Name} {entryPoint?.DeclaringType.FullName}");
            var result = entryPoint.Invoke(null, new object[] { argv });
            if (null == result)
                return 0;
            return (int) result;
        }
    }
}