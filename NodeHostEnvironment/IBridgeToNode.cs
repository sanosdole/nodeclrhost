namespace NodeHostEnvironment
{
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Interface for accessing the node host environment.
    /// </summary>
    public interface IBridgeToNode
    {
        /// <summary>
        /// Returns the global JS object
        /// </summary>
        dynamic Global { get; }

        /// <summary>
        /// Instantiates a new JS object
        /// </summary>
        dynamic New();

        /// <summary>
        /// Checks whether the current thread is the node thread and the bridge can be accessed.
        /// If this is not the case use a <see cref="Run"/> overload to access the bridge.
        /// </summary>
        /// <returns></returns>
        bool CheckAccess();

        /// <summary>
        /// Run a delegate on the node thread.
        /// </summary>
        Task<T> Run<T>(Func<T> func);
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