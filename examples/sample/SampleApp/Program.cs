namespace SampleApp
{
    using System.Threading.Tasks;
    using System;    
    using NodeHostEnvironment;
    using NodeHostEnvironment.NativeHost;

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var host = NativeHost.Initialize();
            var console = host.Global.console;
            console.log("Starting timeout");
            
            // TODO DM 17.09.2019: This crashes as the host has been disposed!
            /*host.Global.setTimeout(new Action(() =>
                                    {
                                        console.log("Timeout from node");
                                        //host.Dispose();
                                    }),
                                   1500);*/
            await RunAsyncApp(host);
            //host.Dispose();
            return 5;
        }

        private static async Task RunAsyncApp(IBridgeToNode host)
        {
            try
            {
                host.Global.console.log($"Hello world from pid:{host.Global.process.pid}!");

                await host.RunAsync(async () =>
                {
                    var global = host.Global;
                    var console = global.console;

                    console.log("Dynamic log from .Net is ", true);

                    console.log("TestClass", global.TestClass.CreateNewInstance("Hallo ctor argument"));

                    global.testCallback(new Func<string, string, string>(MarshalledDelegate), "SecondArg", "ThirdArg");
                    //global.gc();

                    await Task.Delay(100);

                    console.log("DELAYED");

                    var dynInstance = host.New();
                    dynInstance.dynamicProperty1 = "DynProp1";
                    dynInstance.dynamicProperty2 = new Func<string, string, string>(MarshalledDelegate2);
                    // TODO: Why can we not read from the dynamic properties? e.g. dynInstance.dynamicProperty1

                    //global.gc();

                    global.testCallback(new Func<string, string, string>(MarshalledDelegate2), dynInstance, "ThirdArg2");

                    await Task.Delay(100);
                    //global.gc();

                    global.testCallback(new Func<string, string, string>(MarshalledDelegate), "3", dynInstance);
                    //global.gc();

                    global.testCallback(new Func<string, string, string>((a, b) => { console.log("asdas"); return null; }), "3", dynInstance);

                    var tcs = new TaskCompletionSource<object>();
                    global.callLater(new Action(() =>
                    {
                        console.log("We have been called later");
                        tcs.SetResult(null);
                        throw new InvalidOperationException("Test exception");
                    }));

                    Console.WriteLine($"Int from JS {(int)global.testAddon.a}");
                    await tcs.Task;
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                host.Global.console.log($"ByeBye world from pid:{host.Global.process.pid}!");                
            }
        }

        private static string MarshalledDelegate(string a, string b)
        {
            return $".NET has been called with {a ?? "null"} & {b}";
        }

        private static string MarshalledDelegate2(string a, string b)
        {
            return $"2: We have been called with {a ?? "null"} & {b}";
        }
    }
}