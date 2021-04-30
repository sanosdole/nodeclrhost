namespace LocalService
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using NodeHostEnvironment;

    class Program
    {
        static dynamic browserWindow;

        static Task<int> Main(string[] args)
        {
            var host = NodeHost.Instance;

            var tcs = new TaskCompletionSource<int>();

            var console = host.Global.console;

            console.log($"Running broswer app in {host.Global.process.pid}");
            var electron = host.Global.electron;
            
            electron.app.setPath("crashDumps", Path.GetFullPath(Path.Combine(".", "CrashDumps")));
            var crashReporterOptions = host.New();
            crashReporterOptions.submitURL = "https://deadend/"; // Required option even if uploadToServer is false
            crashReporterOptions.uploadToServer = false;
            electron.crashReporter.start(crashReporterOptions);

            electron.app.on("ready",
                            new Action<dynamic>((dynamic launchInfo) =>
                                                {
                                                    console.log("app is ready");
                                                    var options = host.New();
                                                    options.title = ".NET rocks";
                                                    options.backgroundColor = "#fff";
                                                    options.useContentSize = true;
                                                    //options.kiosk = true;

                                                    var webPreferences = host.New();
                                                    options.webPreferences = webPreferences;
                                                    webPreferences.enableRemoteModule = true; // Required for spectron tests, see https://github.com/electron-userland/spectron/pull/738
                                                    webPreferences.contextIsolation = false;
                                                    webPreferences.sandbox = false;

                                                    webPreferences.preload = host.Global.preloadScriptPath;

                                                    //console.log("options:", options);

                                                    browserWindow = electron.BrowserWindow.CreateNewInstance(options);
                                                    
                                                    browserWindow.loadFile("BlazorApp/wwwroot/index.html");
                                                    browserWindow.webContents.on("render-process-gone", new Action<dynamic,dynamic>((_, details) => {
                                                        Console.WriteLine($"Renderer gone: {(string)details.reason}");
                                                        browserWindow.close();
                                                    }));
                                                }));

            electron.app.on("will-quit",
                            new Action<dynamic>(e =>
                                                {
                                                    tcs.SetResult(5);
                                                }));

            return tcs.Task;
        }
    }
}
