namespace MvcApp
{
   using System;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Reflection;
   using System.Threading;
   using System.Threading.Tasks;
   using Microsoft.AspNetCore.Hosting;
   using Microsoft.Extensions.Hosting;
   using NodeHostEnvironment;

   public class Program
   {
      private static dynamic browserWindow;
      public static async Task Main(string[] args)
      {
         //if (args.Any(arg => arg.Equals("-debug")))
         // Debugger.Launch();
         //Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
         //Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
         var node = NodeHost.Instance;
         try
         {
            using var stopSource = new CancellationTokenSource();
            var electron = node.Global.electron;
            electron.app.on("ready", new Action<dynamic>((dynamic launchInfo) =>
                                                         {
                                                            var options = node.New();
                                                            options.title = ".NET rocks";
                                                            options.backgroundColor = "#fff";
                                                            options.useContentSize = true;
                                                            //options.kiosk = true;

                                                            var webPreferences = node.New();
                                                            options.webPreferences = webPreferences;
                                                            webPreferences.nodeIntegration = true;
                                                            /*webPreferences.contextIsolation = false;
                                                               webPreferences.sandbox = false;
                                                               webPreferences.devTools = false;*/

                                                            //console.log("options:", options);

                                                            browserWindow = electron.BrowserWindow.CreateNewInstance(options);

                                                            browserWindow.loadURL("https://localhost:5001/");

                                                         }));

            electron.app.on("will-quit", new Action<dynamic>(e =>
                                                             {
                                                                stopSource.Cancel();
                                                                e.preventDefault();
                                                             }));

            var webHost = CreateHostBuilder(args).Build();
            await webHost.RunAsync(token: stopSource.Token);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
         node.Global.process.exit();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .UseContentRoot(args[0]) // TODO DM 27.05.2020: Why is this necessary?
             .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
   }
}
