namespace ElectronHostedBlazor.Logging
{
    using Microsoft.Extensions.Logging;
    using NodeHostEnvironment;

    internal sealed class ElectronLoggerProvider : ILoggerProvider
    {
        private readonly ElectronConsoleWriter _writer;

        public ElectronLoggerProvider(IBridgeToNode node)
        {
            _writer = new ElectronConsoleWriter(node);
        }

        public ILogger CreateLogger(string categoryName)
        {
            // We need to track them once we support options
            return new ElectronConsoleLogger(_writer, categoryName);
        }

        public void Dispose()
        { }
    }
}