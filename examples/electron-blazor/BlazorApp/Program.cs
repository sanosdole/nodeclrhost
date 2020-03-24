namespace BlazorApp
{
    using ElectronHostedBlazor.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

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