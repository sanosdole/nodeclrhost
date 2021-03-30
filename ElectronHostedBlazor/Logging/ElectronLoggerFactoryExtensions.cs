namespace ElectronHostedBlazor.Logging
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;

    public static class ElectronLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds an electron console logger named 'ElectronConsole' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddElectronConsole(this ILoggingBuilder builder)
        {
            //builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ElectronLoggerProvider>());
            //LoggerProviderOptions.RegisterProviderOptions<ElectronLoggerOptions, ElectronLoggerProvider>(builder.Services);            
            return builder;
        }

        /*
        /// <summary>
        /// Adds an electron console logger named 'ElectronConsole' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure">A delegate to configure the <see cref="ElectronConsoleLogger"/>.</param>
        public static ILoggingBuilder AddConsole(this ILoggingBuilder builder, Action<ElectronLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddElectronConsole();
            builder.Services.Configure(configure);

            return builder;
        }
        */
    }
}
