namespace NodeHostEnvironment.InProcess
{
   using System;
   using System.Linq;

   internal static class InvokeHelper
   {
      public static DotNetCallback CreateInvoker(Delegate @delegate, IHostInProcess host)
      {
         var method = @delegate.Method;
         var requiredParameters = method.GetParameters();
         switch (requiredParameters.Length)
         {
            case 0:
               if (method.ReturnType == typeof(void))
                  return new InvokerVoid(@delegate, host).Callback;
               else
                  // ReSharper disable once PossibleNullReferenceException
                  return ((AbstractInvoker)Activator.CreateInstance(typeof(Invoker<>).MakeGenericType(method.ReturnType),
                                                                    @delegate,
                                                                    host)).Callback;
            case 1:
               if (method.ReturnType == typeof(void))
                  // ReSharper disable once PossibleNullReferenceException
                  return ((AbstractInvoker)Activator.CreateInstance(typeof(InvokerVoid<>).MakeGenericType(requiredParameters[0].ParameterType),
                                                                    @delegate,
                                                                    host)).Callback;
               else
                  // ReSharper disable once PossibleNullReferenceException
                  return ((AbstractInvoker)Activator.CreateInstance(typeof(Invoker<,>).MakeGenericType(requiredParameters[0].ParameterType,
                                                                                                       method.ReturnType),
                                                                    @delegate,
                                                                    host)).Callback;
            case 2:
               if (method.ReturnType == typeof(void))
                  // ReSharper disable once PossibleNullReferenceException
                  return ((AbstractInvoker)Activator.CreateInstance(typeof(InvokerVoid<,>).MakeGenericType(requiredParameters[0].ParameterType,
                                                                                                           requiredParameters[1].ParameterType),
                                                                    @delegate,
                                                                    host)).Callback;
               else
                  // ReSharper disable once PossibleNullReferenceException
                  return ((AbstractInvoker)Activator.CreateInstance(typeof(Invoker<,,>).MakeGenericType(requiredParameters[0].ParameterType,
                                                                                                        requiredParameters[1].ParameterType,
                                                                                                        method.ReturnType),
                                                                    @delegate,
                                                                    host)).Callback;
            default:
               // Fallback to reflection
               return argv => DynamicInvoke(@delegate, host, argv);
         }
      }

      private abstract class AbstractInvoker
      {
         protected readonly IHostInProcess Host;

         protected AbstractInvoker(IHostInProcess host)
         {
            Host = host;
         }

         public abstract DotNetCallback Callback { get; }

         protected TArg GetTypedArgument<TArg>(JsValue[] argv, int index)
         {
            var jsArg = argv[index];
            if (!jsArg.TryGetObject(Host, typeof(TArg), out var result))
            {
               ReleaseRemainingArgs(argv, index);
               throw new InvalidOperationException($"Cannot get {typeof(TArg).FullName} from JS handle of type {jsArg.Type}");
            }

            var a1 = (TArg)result;
            return a1;
         }

         protected void ReleaseRemainingArgs(JsValue[] argv, int index)
         {
            for (var c = index; c < argv.Length; c++)
               Host.Release(argv[c]);
         }
      }

      private sealed class InvokerVoid : AbstractInvoker
      {
         private readonly Action _action;

         public InvokerVoid(Delegate @delegate, IHostInProcess host) : base(host)
         {
            _action = @delegate as Action
                      ?? (Action)Delegate.CreateDelegate(typeof(Action), @delegate.Target, @delegate.Method);
         }

         public override DotNetCallback Callback => InvokeDelegate;

         private DotNetValue InvokeDelegate(JsValue[] argv)
         {
            ReleaseRemainingArgs(argv, 0);

            _action();
            return DotNetValue.NullValue;
         }
      }

      private sealed class Invoker<TResult> : AbstractInvoker
      {
         private readonly Func<TResult> _func;

         public Invoker(Delegate @delegate, IHostInProcess host) : base(host)
         {
            _func = @delegate as Func<TResult>
                    ?? (Func<TResult>)Delegate.CreateDelegate(typeof(Func<TResult>), @delegate.Target, @delegate.Method);
         }

         public override DotNetCallback Callback => InvokeDelegate;

         private DotNetValue InvokeDelegate(JsValue[] argv)
         {
            ReleaseRemainingArgs(argv, 0);

            return DotNetValue.FromObject(_func(), Host);
         }
      }

      private sealed class InvokerVoid<T1> : AbstractInvoker
      {
         private readonly Action<T1> _action;

         public InvokerVoid(Delegate @delegate, IHostInProcess host) : base(host)
         {
            _action = @delegate as Action<T1>
                      ?? (Action<T1>)Delegate.CreateDelegate(typeof(Action<T1>), @delegate.Target, @delegate.Method);
         }

         public override DotNetCallback Callback => InvokeDelegate;

         private DotNetValue InvokeDelegate(JsValue[] argv)
         {
            if (argv.Length < 1)
            {
               ReleaseRemainingArgs(argv, 0);
               throw new InvalidOperationException("We need at least 1 argument!");
            }

            var a1 = GetTypedArgument<T1>(argv, 0);

            ReleaseRemainingArgs(argv, 1);

            _action(a1);
            return DotNetValue.NullValue;
         }
      }

      private sealed class Invoker<T1, TResult> : AbstractInvoker
      {
         private readonly Func<T1, TResult> _func;

         public Invoker(Delegate @delegate, IHostInProcess host) : base(host)
         {
            _func = @delegate as Func<T1, TResult>
                    ?? (Func<T1, TResult>)Delegate.CreateDelegate(typeof(Func<T1, TResult>), @delegate.Target, @delegate.Method);
         }

         public override DotNetCallback Callback => InvokeDelegate;

         private DotNetValue InvokeDelegate(JsValue[] argv)
         {
            if (argv.Length < 1)
            {
               ReleaseRemainingArgs(argv, 0);
               throw new InvalidOperationException("We need at least 1 argument!");
            }

            var a1 = GetTypedArgument<T1>(argv, 0);

            ReleaseRemainingArgs(argv, 1);

            return DotNetValue.FromObject(_func(a1), Host);
         }
      }

      private sealed class InvokerVoid<T1, T2> : AbstractInvoker
      {
         private readonly Action<T1, T2> _action;

         public InvokerVoid(Delegate @delegate, IHostInProcess host) : base(host)
         {
            _action = @delegate as Action<T1, T2>
                      ?? (Action<T1, T2>)Delegate.CreateDelegate(typeof(Action<T1, T2>), @delegate.Target, @delegate.Method);
         }

         public override DotNetCallback Callback => InvokeDelegate;

         private DotNetValue InvokeDelegate(JsValue[] argv)
         {
            if (argv.Length < 2)
            {
               ReleaseRemainingArgs(argv, 0);
               throw new InvalidOperationException("We need at least 2 argument!");
            }

            var a1 = GetTypedArgument<T1>(argv, 0);
            var a2 = GetTypedArgument<T2>(argv, 1);

            ReleaseRemainingArgs(argv, 2);

            _action(a1, a2);
            return DotNetValue.NullValue;
         }
      }

      private sealed class Invoker<T1, T2, TResult> : AbstractInvoker
      {
         private readonly Func<T1, T2, TResult> _func;

         public Invoker(Delegate @delegate, IHostInProcess host) : base(host)
         {
            _func = @delegate as Func<T1, T2, TResult>
                    ?? (Func<T1, T2, TResult>)Delegate.CreateDelegate(typeof(Func<T1, T2, TResult>), @delegate.Target, @delegate.Method);
         }

         public override DotNetCallback Callback => InvokeDelegate;

         private DotNetValue InvokeDelegate(JsValue[] argv)
         {
            if (argv.Length < 2)
            {
               ReleaseRemainingArgs(argv, 0);
               throw new InvalidOperationException("We need at least 2 argument!");
            }

            var a1 = GetTypedArgument<T1>(argv, 0);
            var a2 = GetTypedArgument<T2>(argv, 1);

            ReleaseRemainingArgs(argv, 2);

            return DotNetValue.FromObject(_func(a1, a2), Host);
         }
      }

      private static DotNetValue DynamicInvoke(Delegate @delegate, IHostInProcess host, JsValue[] argv)
      {
         var requiredParameters = @delegate.Method.GetParameters();
         if (requiredParameters.Length > argv.Length)
         {
            foreach (var toRelease in argv)
               host.Release(toRelease);
            // This exception will be passed properly to JS
            throw new InvalidOperationException($"We need at least {requiredParameters.Length} arguments!");
         }

         var mappedArgs = new object[requiredParameters.Length];
         for (var c = 0; c < requiredParameters.Length; c++)
         {
            var paramType = requiredParameters[c].ParameterType;
            if (!argv[c].TryGetObject(host, paramType, out object parameter))
            {
               // Release remaining arguments
               foreach (var toRelease in argv.Skip(c + 1))
                  host.Release(toRelease);
               throw new InvalidOperationException($"Cannot get {paramType.FullName} from JS handle of type {argv[c].Type}");
            }

            mappedArgs[c] = parameter;
         }

         // Release remaining arguments
         foreach (var toRelease in argv.Skip(mappedArgs.Length))
            host.Release(toRelease);

         var resultObj = @delegate.DynamicInvoke(mappedArgs);
         return DotNetValue.FromObject(resultObj, host);
      }
   }
}
