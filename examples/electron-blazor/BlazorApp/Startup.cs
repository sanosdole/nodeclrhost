namespace BlazorApp
{
    using ElectronHostedBlazor.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) { }

        public void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddDebug()
                          .AddFilter("Microsoft", LogLevel.Debug)
                          .AddFilter("System", LogLevel.Warning)
                          .AddFilter("BlazorApp", LogLevel.Debug);
        }

        public void Configure(RootComponentMappingCollection app)
        {
            app.Add<App>("app");
        }
    }
}
