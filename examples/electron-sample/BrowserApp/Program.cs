namespace BrowserApp
{
    using System.Threading.Tasks;
    using System;
    using System.IO;
    using NodeHostEnvironment;

    class Program
    {
        static dynamic browserWindow;
        static Task<int> Main(string[] args)
        {
            var tcs = new TaskCompletionSource<int>();
            var host = NodeHost.Instance;
            var console = host.Global.console;

            console.log($"Running broswer app2 in {host.Global.process.pid}");
            var electron = host.Global.electron;
            electron.app.on("ready", new Action<dynamic>((dynamic launchInfo) =>
            {
                console.log("app is ready");
                var options = host.New();
                options.title = ".NET rocks";
                var webPreferences = host.New();
                options.webPreferences = webPreferences;
                webPreferences.preload = Path.Combine(host.Global.appRoot, "preload.js");
                webPreferences.contextIsolation = false;
                webPreferences.sandbox = false;
                webPreferences.devTools = true;

                //console.log("options:", options);

                browserWindow = electron.BrowserWindow.CreateNewInstance(options);

                browserWindow.loadFile("renderer.html");
            }));

            electron.app.on("will-quit", new Action<dynamic>(e =>
            {
                tcs.SetResult(5);

            }));
            return tcs.Task;
        }
    }
}