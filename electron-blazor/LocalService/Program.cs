namespace LocalService
{
    using System;
    using NodeHostEnvironment;

    class Program
    {
        static dynamic browserWindow;
        static int Main(string[] args)
        {
            var host = NodeHost.InProcess();
            try
            {
                var console = host.Global.console;
                
                console.log($"Running broswer app in {host.Global.process.pid}");
                var electron = host.Global.electron;
                electron.app.on("ready", new Action<dynamic>((dynamic launchInfo) =>
                {
                    console.log("app is ready");
                    var options = host.New();
                    options.title = ".NET rocks";
                    options.backgroundColor = "#fff";
                    options.useContentSize = true;
                    //options.kiosk = true;

                    var webPreferences = host.New();
                    options.webPreferences = webPreferences;
                    webPreferences.nodeIntegration = true;
                    /*webPreferences.contextIsolation = false;
                    webPreferences.sandbox = false;
                    webPreferences.devTools = false;*/

                    //console.log("options:", options);
                    
                    browserWindow = electron.BrowserWindow.CreateNewInstance(options);
                                        
                    browserWindow.loadFile("BlazorApp/wwwroot/index.html");

                }));

                electron.app.on("will-quit", new Action<dynamic>(e =>
                {
                    host.Dispose();

                }));

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            return 5;
        }
    }
}
