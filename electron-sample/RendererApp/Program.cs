namespace RendererApp
{
    using System.Threading.Tasks;
    using System;
    using NodeHostEnvironment;
    using NodeHostEnvironment.NativeHost;
    using System.Diagnostics;

    class Program
    {
        static int Main(string[] args)
        {
            var host = NativeHost.Initialize();

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
                    var div = host.Global.window.document.getElementById("AnimateDiv");
                    for (var c = 0; c < 30; c++)
                    {
                        await Task.Delay(1000);
                        //host.Global.console.log("Replacing ", c);  
                        for (var c2 = 0; c2 < 1000; c2++)                      
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