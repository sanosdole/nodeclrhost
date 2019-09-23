using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace NodeHostEnvironment.NativeHost
{
    internal static class DynamicLibraryLoader
    {
        public static T LoadApi<T>(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName))
                throw new ArgumentException("libraryName must not be null or empty", nameof(libraryName));

            var loader = SelectLoader();

            // DM 11.09.2019: We do not unload the library, as it is loaded already and we only live as long as it is loaded...
            var libraryHandle = loader.LoadLibrary(libraryName);
            if (libraryHandle == IntPtr.Zero)
                throw new InvalidOperationException($"Could not load library '{libraryName}");

            var ctor = typeof(T).GetConstructors().Single();
            var parameters = ctor.GetParameters();
            var arguments = new object[parameters.Length];

            for (int c = 0; c < parameters.Length; c++)
            {
                var type = parameters[c].ParameterType;
                // TODO DM 11.09.2019: Assert that type is delegate with the proper attribute
                arguments[c] = Marshal.GetDelegateForFunctionPointer(loader.LoadSymbol(libraryHandle, type.Name), type);
            }

            return (T) Activator.CreateInstance(typeof(T), args : arguments);
        }

        private interface ILoader
        {
            IntPtr LoadLibrary(string name);
            IntPtr LoadSymbol(IntPtr library, string name);
        }
        private static ILoader SelectLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPlatformLoader();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxPlatformLoader();
            }

            /*
                Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
                "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
            */
            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || isBSD)
            {
                return new BSDPlatformLoader();
            }

            throw new InvalidOperationException("We do not support your platform!");
        }

        private sealed class WindowsPlatformLoader : ILoader
        {
            public IntPtr LoadLibrary(string name)
            {
                return NativeMethods.LoadLibrary(name);
            }

            public IntPtr LoadSymbol(IntPtr library, string name)
            {
                return NativeMethods.GetProcAddress(library, name);
            }

            [SuppressUnmanagedCodeSecurity]
            private static class NativeMethods
            {
                [DllImport("kernel32")]
                public static extern IntPtr LoadLibrary(string fileName);

                [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)]
                public static extern IntPtr GetProcAddress(IntPtr module, string procName);

                [DllImport("kernel32")]
                public static extern int FreeLibrary(IntPtr module);
            }
        }

        private sealed class LinuxPlatformLoader : ILoader
        {
            public IntPtr LoadLibrary(string name)
            {
                return NativeMethods.dlopen(name, SymbolFlag.RTLD_DEFAULT);
            }

            public IntPtr LoadSymbol(IntPtr library, string name)
            {
                return NativeMethods.dlsym(library, name);
            }
            private const string LibraryNameUnix = "dl";

            [SuppressUnmanagedCodeSecurity]
            private static class NativeMethods
            {
                [DllImport(LibraryNameUnix)]
                public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

                [DllImport(LibraryNameUnix)]
                public static extern IntPtr dlsym(IntPtr handle, string name);

                [DllImport(LibraryNameUnix)]
                public static extern int dlclose(IntPtr handle);

                [DllImport(LibraryNameUnix)]
                public static extern IntPtr dlerror();
            }
        }

        private sealed class BSDPlatformLoader : ILoader
        {
            public IntPtr LoadLibrary(string name)
            {
                return NativeMethods.dlopen(name, SymbolFlag.RTLD_DEFAULT);
            }

            public IntPtr LoadSymbol(IntPtr library, string name)
            {
                return NativeMethods.dlsym(library, name);
            }

            private const string LibraryNameBSD = "c";

            [SuppressUnmanagedCodeSecurity]
            private static class NativeMethods
            {
                [DllImport(LibraryNameBSD)]
                public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

                [DllImport(LibraryNameBSD)]
                public static extern IntPtr dlsym(IntPtr handle, string name);

                [DllImport(LibraryNameBSD)]
                public static extern int dlclose(IntPtr handle);

                [DllImport(LibraryNameBSD)]
                public static extern IntPtr dlerror();
            }

        }

        /// <summary>
        /// <see cref="dl.open"/> flags. Taken from the source code of GNU libc.
        ///
        /// <a href="https://github.com/lattera/glibc/blob/master/bits/dlfcn.h"/>
        /// </summary>
        [Flags]
        private enum SymbolFlag
        {
            /// <summary>
            /// The default flags.
            /// </summary>
            RTLD_DEFAULT = RTLD_NOW,

            /// <summary>
            /// Lazy function call binding.
            /// </summary>
            RTLD_LAZY = 0x00001,

            /// <summary>
            /// Immediate function call binding.
            /// </summary>
            RTLD_NOW = 0x00002,

            /// <summary>
            /// If set, makes the symbols of the loaded object and its dependencies visible
            /// as if the object was linked directly into the program.
            /// </summary>
            RTLD_GLOBAL = 0x00100,

            /// <summary>
            /// The inverse of <see cref="RTLD_GLOBAL"/>. Typically, this is the default behaviour.
            /// </summary>
            RTLD_LOCAL = 0x00000,

            /// <summary>
            /// Do not delete the object when closed.
            /// </summary>
            RTLD_NODELETE = 0x01000
        }
    }
}