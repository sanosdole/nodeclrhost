namespace NodeHostEnvironment.InProcess
{
   using System;
   using System.Collections.Generic;
   using System.Runtime.InteropServices;
   using System.Threading.Tasks;

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void ReleaseDotNetValue(DotNetType type, IntPtr value);

   [StructLayout(LayoutKind.Sequential)]
   internal struct DotNetValue
   {
      public DotNetType Type;
      public IntPtr Value;
      public ReleaseDotNetValue ReleaseFunc;

      public static DotNetValue FromObject(object obj, IHostInProcess host)
      {
         if (null == obj)
            return NullValue;

         // Basic types
         if (obj is string @string)
            return FromString(@string);
         if (obj is bool boolean)
            return FromBool(boolean);
         if (obj is int @int)
            return FromInt(@int);
         if (obj is long @long)
            return FromLong(@long);
         if (obj is double @double)
            return FromDouble(@double);

         if (obj is Task task)
            return FromTask(task, host);

         // Objects & Functions
         if (obj is Delegate @delegate)
            return FromDelegate(@delegate, host);
         if (obj is JsDynamicObject @object)
            return FromJsValue(@object.Handle);
         if (obj is JsValue value)
            return FromJsValue(value);

         // Specials
         if (obj is byte[] byteArray)
            return FromByteArray(byteArray);
         if (obj is NativeMemory nativeMemory)
            return FromNativeMemory(nativeMemory);

         if (obj is IReadOnlyCollection<object> collection)
            return FromReadOnlyCollection(collection, host);

         if (obj is Exception exception)
            return FromException(exception);

         throw new InvalidOperationException($"Unsupported object type for passing into JS: {obj.GetType().FullName}");
      }

      public static DotNetValue FromDelegate(Delegate @delegate, IHostInProcess host)
      {
         var value = host.MarshallCallback(InvokeHelper.CreateInvoker(@delegate, host), out var releaseCallback);

         return new DotNetValue
                {
                   Type = DotNetType.Function,
                   Value = value,
                   ReleaseFunc = releaseCallback
                };
      }

      public static DotNetValue FromJsValue(JsValue value)
      {
         var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));
         Marshal.StructureToPtr(value, ptr, false);
         return new DotNetValue
                {
                   Type = DotNetType.JsHandle,
                   Value = ptr,
                   ReleaseFunc = ReleaseHGlobal
                };
      }

      public static DotNetValue FromString(string value)
      {
         return new DotNetValue
                {
                   Type = DotNetType.String,
                   Value = NativeUtf8FromString(value),
                   ReleaseFunc = ReleaseString
                };
      }

      public static DotNetValue FromBool(bool value)
      {
         return new DotNetValue
                {
                   Type = DotNetType.Boolean,
                   Value = value ? new IntPtr(1) : IntPtr.Zero,
                   ReleaseFunc = null
                };
      }

      public static DotNetValue FromInt(int value)
      {
         return new DotNetValue
                {
                   Type = DotNetType.Int32,
                   Value = new IntPtr(value),
                   ReleaseFunc = null
                };
      }

      public static DotNetValue FromLong(long value)
      {
         return new DotNetValue
                {
                   Type = DotNetType.Int32,
                   Value = new IntPtr(value),
                   ReleaseFunc = null
                };
      }

      public static DotNetValue FromDouble(double value)
      {
         return new DotNetValue
                {
                   Type = DotNetType.Double,
                   // TODO: Breaks on 32bit node :(
                   Value = new IntPtr(BitConverter.DoubleToInt64Bits(value)),
                   ReleaseFunc = null
                };
      }

      public static DotNetValue FromByteArray(byte[] value)
      {
         return new DotNetValue
                {
                   Type = DotNetType.ByteArray,
                   Value = ArrayPointer(value),
                   ReleaseFunc = ReleaseArrayPointer
                };
      }

      public static DotNetValue FromNativeMemory(NativeMemory value)
      {
         var gcHandle = GCHandle.Alloc(value, GCHandleType.Normal);

         // Memory layout: |int size|IntPtr data|IntPtr gcHandle|
         var structPtr = Marshal.AllocHGlobal(sizeof(int) + 2 * IntPtr.Size);
         Marshal.WriteInt32(structPtr, value.Length);
         Marshal.WriteIntPtr(structPtr, sizeof(int), value.Pointer);
         Marshal.WriteIntPtr(structPtr, sizeof(int) + IntPtr.Size, GCHandle.ToIntPtr(gcHandle));

         return new DotNetValue
                {
                   Type = DotNetType.ByteArray,
                   Value = structPtr,
                   ReleaseFunc = ReleaseNativeMemory
                };
      }

      private static DotNetValue FromReadOnlyCollection(IReadOnlyCollection<object> collection, IHostInProcess host)
      {
         return new DotNetValue
                {
                   Type = DotNetType.Collection,
                   Value = CollectionPointer(collection, host),
                   ReleaseFunc = ReleaseHGlobal
                };
      }

      public static DotNetValue FromException(Exception value)
      {
         return new DotNetValue
                {
                   Type = DotNetType.Exception,
                   Value = NativeUtf8FromString($"{value.GetType().Name}: {value.Message}\n{value.StackTrace}"),
                   ReleaseFunc = ReleaseString
                };
      }

      public static DotNetValue FromTask(Task value, IHostInProcess host)
      {
         return new DotNetValue
                {
                   Type = DotNetType.Task,
                   Value = host.MarshallTask(value),
                   ReleaseFunc = null
                };
      }

      private static readonly ReleaseDotNetValue ReleaseHGlobal = ReleaseHGlobalIntern;

      private static readonly ReleaseDotNetValue ReleaseString = ReleaseStringIntern;
      private static readonly ReleaseDotNetValue ReleaseArrayPointer = ReleaseArrayPointerIntern;
      private static readonly ReleaseDotNetValue ReleaseNativeMemory = ReleaseNativeMemoryIntern;

      public static readonly DotNetValue NullValue = new DotNetValue
                                                     {
                                                        Type = DotNetType.Null,
                                                        Value = IntPtr.Zero,
                                                        ReleaseFunc = null
                                                     };

      private static void ReleaseHGlobalIntern(DotNetType type, IntPtr value)
      {
         Marshal.FreeHGlobal(value);
      }

      private static IntPtr NativeUtf8FromString(string managedString)
      {
         var gcHandle = GCHandle.Alloc(managedString, GCHandleType.Pinned);
         var dataPtr = gcHandle.AddrOfPinnedObject();
         var structPtr = Marshal.AllocHGlobal(sizeof(int) + 2 * IntPtr.Size);
         Marshal.WriteInt32(structPtr, managedString.Length);
         Marshal.WriteIntPtr(structPtr, sizeof(int), dataPtr);
         Marshal.WriteIntPtr(structPtr, sizeof(int) + IntPtr.Size, GCHandle.ToIntPtr(gcHandle));
         return structPtr;
      }

      private static void ReleaseStringIntern(DotNetType type, IntPtr value)
      {
         var gcHandlePtr = Marshal.ReadIntPtr(value, sizeof(int) + IntPtr.Size);
         var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
         gcHandle.Free();
         Marshal.FreeHGlobal(value);
      }

      private static void ReleaseArrayPointerIntern(DotNetType type, IntPtr value)
      {
         var gcHandlePtr = Marshal.ReadIntPtr(value, sizeof(int) + IntPtr.Size);
         var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
         gcHandle.Free();
         Marshal.FreeHGlobal(value);
      }

      private static void ReleaseNativeMemoryIntern(DotNetType type, IntPtr value)
      {
         var gcHandlePtr = Marshal.ReadIntPtr(value, sizeof(int) + IntPtr.Size);
         var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
         var memory = (NativeMemory)gcHandle.Target;
         gcHandle.Free();
         Marshal.FreeHGlobal(value);
         memory.Dispose();
      }

      private static IntPtr ArrayPointer(byte[] array)
      {
         var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
         var dataPtr = gcHandle.AddrOfPinnedObject();
         // Memory layout: |int size|IntPtr data|IntPtr gcHandle|
         var structPtr = Marshal.AllocHGlobal(sizeof(int) + 2 * IntPtr.Size);
         Marshal.WriteInt32(structPtr, array.Length);
         Marshal.WriteIntPtr(structPtr, sizeof(int), dataPtr);
         Marshal.WriteIntPtr(structPtr, sizeof(int) + IntPtr.Size, GCHandle.ToIntPtr(gcHandle));
         return structPtr;
      }

      private static IntPtr CollectionPointer(IReadOnlyCollection<object> collection, IHostInProcess host)
      {
         var sizeOfDotnetValue = Marshal.SizeOf(typeof(DotNetValue));
         var result = Marshal.AllocHGlobal(sizeof(int) + sizeOfDotnetValue * collection.Count);
         try
         {
            Marshal.WriteInt32(result, collection.Count);
            var writePtr = result + sizeof(int);
            foreach (var value in collection)
            {
               var wrapped = FromObject(value, host);
               Marshal.StructureToPtr(wrapped, writePtr, false);
               writePtr += sizeOfDotnetValue;
            }
         }
         catch
         {
            Marshal.FreeHGlobal(result);
            throw;
         }

         return result;
      }
   }
}
