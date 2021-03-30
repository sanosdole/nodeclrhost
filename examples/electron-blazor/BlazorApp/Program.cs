namespace BlazorApp
{
    using System.Threading.Tasks;
    using ElectronHostedBlazor.Hosting;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            return CreateHostBuilder(args).Build().Run();
        }

        public static IElectronHostBuilder CreateHostBuilder(string[] args) =>
            BlazorElectronHost.CreateDefaultBuilder()
                              .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
                              .UseBlazorStartup<Startup>();
    }
}
