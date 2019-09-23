using BlazorApp.Hosting;

namespace BlazorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            CreateHostBuilder(args).Build().Run();
        }

        public static INodeHostBuilder CreateHostBuilder(string[] args) =>
            BlazorNodeHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }
}
