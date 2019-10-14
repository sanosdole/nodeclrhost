using Microsoft.Extensions.Logging;

namespace BlazorApp.Services
{
    internal class NodeLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            // No-op
        }

        public ILogger CreateLogger(string categoryName)
            => new NodeConsoleLogger<object>();

        public void Dispose()
        {
            // No-op
        }
    }
}