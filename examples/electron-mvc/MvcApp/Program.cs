namespace MvcApp
{
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   using Microsoft.AspNetCore.Hosting;
   using Microsoft.Extensions.Hosting;
   using NodeHostEnvironment;

   public class Program
   {
      private static dynamic browserWindow;

      public static async Task<int> Main(string[] args)
      {
         //if (args.Any(arg => arg.Equals("-debug")))
         // Debugger.Launch();
         //Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
         //Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
         var node = NodeHost.Instance;

         using var stopSource = new CancellationTokenSource();
         var electron = node.Global.require("electron");
         electron.app.on("ready",
                         new Action<dynamic>((dynamic launchInfo) =>
                                             {
                                                var options = node.New();
                                                options.title = ".NET rocks";
                                                options.backgroundColor = "#fff";
                                                options.useContentSize = true;

                                                var webPreferences = node.New();
                                                options.webPreferences = webPreferences;
                                                webPreferences.nodeIntegration = true;

                                                browserWindow = electron.BrowserWindow.CreateNewInstance(options);

                                                // TODO DM 27.05.2020: Get port, ensure trusted certificates
                                                browserWindow.loadURL("https://localhost:5001/");
                                             }));

         electron.app.on("will-quit",
                         new Action<dynamic>(e =>
                                             {
                                                stopSource.Cancel();
                                                e.preventDefault();
                                             }));

         var webHost = CreateHostBuilder(args).Build();
         await webHost.RunAsync(token: stopSource.Token);

         return 0;
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .UseContentRoot(args[0]) // DM 27.05.2020: This is necessary as static files are based on current directory
             .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
   }
}
