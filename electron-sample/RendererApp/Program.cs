namespace RendererApp
{
    using System.Threading.Tasks;
    using System;
    using NodeHostEnvironment;
    using System.Diagnostics;

    class Program
    {
        static int Main(string[] args)
        {
            var host = NodeHost.InProcess();

            try
            {
                var console = host.Global.console;

                console.log($"Running renderer app in {host.Global.process.pid}");

                host.Global.window.addEventListener("unload", new Action<dynamic>(e =>
                {
                    host.Dispose();
                }));

                AnimateTest(host);
            }
            catch (Exception e)
            {
                host.Global.console.log($".NET exception: {e}");
            }
            finally
            {
                // This will lead to node closing and future callbacks being rejected
                //host.Dispose();
            }

            return 5;
        }

        private static async void AnimateTest(NodeHost host)
        {
            await host.Run(async() =>
            {
                try
                {
                    //System.Diagnostics.Debugger.Launch();
                    var div = host.Global.window.document.getElementById("AnimateDiv");
                    for (var c = 0; c < 30; c++)
                    {
                        // Merge test
                        await Task.Delay(1000);
                        //host.Global.console.log("Replacing ", c);                        
                        div.innerHTML = $@"{c} replaced @ {DateTime.Now} from {Process.GetCurrentProcess().Id}";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                }
            });
        }
    }
}