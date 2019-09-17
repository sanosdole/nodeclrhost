namespace NodeHostEnvironment.BridgeApi
{
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Interface for accessing the node host environment.
    /// </summary>
    public interface IBridgeToNode : IDisposable
    {
        dynamic Global { get; }
        dynamic New();

        Task<T> Run<T>(Func<T> func);

        /* TODO: Default interface implementation requires dotnet 3.0 Preview 9+ 
                public async Task<T> Run<T>(Func<Task<T>> action)
                {
                    return await Run(action);
                }

                public Task Run(Func<Task> action)
                {
                    return Run<Task>(action).Unwrap();
                }

                public Task Run(Action action)
                {
                    return Run(() => { action(); return (object)null; });
                }
        */
    }

    /// <summary>
    /// Extensions for <see cref="IBridgeToNode"/> that should be default interface implementations.
    /// </summary>
    public static class BridgeToNodeExtensions
    {
        public static Task<T> RunAsync<T>(this IBridgeToNode thiz, Func<Task<T>> asyncFunc)
        {
            return thiz.Run(asyncFunc).Unwrap();
        }

        public static Task RunAsync(this IBridgeToNode thiz, Func<Task> asyncAction)
        {
            return thiz.Run(asyncAction).Unwrap();
        }

        public static Task Run(this IBridgeToNode thiz, Action action)
        {
            return thiz.Run(() =>
            {
                action();
                return (object) null;
            });
        }
    }
}