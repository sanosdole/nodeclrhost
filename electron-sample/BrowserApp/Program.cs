namespace BrowserApp
{
    using System.Threading.Tasks;
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
                    var webPreferences = host.New();
                    options.webPreferences = webPreferences;
                    webPreferences.nodeIntegration = true;
                    webPreferences.contextIsolation = false;
                    webPreferences.sandbox = false;
                    webPreferences.devTools = false;

                    //console.log("options:", options);
                    
                    browserWindow = electron.BrowserWindow.CreateNewInstance(options);
                    
                    // TODO: Either the second parameter or the returned promise are a problem here!
                    browserWindow.loadFile("renderer.html");

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