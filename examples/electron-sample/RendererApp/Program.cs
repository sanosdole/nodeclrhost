namespace RendererApp
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System;
    using NodeHostEnvironment;

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var tcs = new TaskCompletionSource<int>();
            var host = NodeHost.Instance;
            var console = host.Global.console;

            console.log($"Running renderer app in {host.Global.myApi.process.pid}");

            host.Global.window.addEventListener("unload", new Action<dynamic>(e =>
            {
                tcs.SetResult(5);
            }));

            await host.Run(async() =>
            {
                var div = host.Global.window.document.getElementById("AnimateDiv");
                for (var c = 0; c < 30; c++)
                {
                    await Task.Delay(1000);
                    //host.Global.console.log("Replacing ", c);  
                    for (var c2 = 0; c2 < 1000; c2++)
                        div.innerHTML = $@"{c} replaced @ {DateTime.Now} from {Process.GetCurrentProcess().Id}";
                }

            });

            return await tcs.Task;
        }
    }
}