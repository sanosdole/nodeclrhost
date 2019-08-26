using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlazorApp.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Logging;

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
