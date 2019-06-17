namespace NodeHostEnvironment.BridgeApi
{
    using System.Threading.Tasks;
    using System;

    public interface IBridgeToNode : IDisposable
    {
        dynamic Global { get; }
        dynamic New();

        Task<T> Run<T>(Func<T> action);
    }
}