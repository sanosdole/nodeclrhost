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

            var builder = ElectronHostBuilder.CreateDefault(args);
            builder.Logging.AddConsole();
            var startup = new Startup();
            startup.ConfigureServices(builder.Services);
            startup.ConfigureLogging(builder.Logging);
            startup.Configure(builder.RootComponents);

            return builder.Build().Run();
        }
    }
}
