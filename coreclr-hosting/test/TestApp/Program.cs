using System;

namespace TestApp
{
    using NodeHostEnvironment;
    class Program
    {
        static void Main(string[] args)
        {
            var host = NodeHost.InProcess();
            var global = host.Global;
            try
            {
                /*Console.WriteLine("describe");
                global.describe("First test", new Action(() =>
                {
                    Console.WriteLine("it");
                    global.it("should do sth", new Action(() =>
                    {
                        Console.WriteLine("assert");
                        //global.assert.equal(4,4);
                    }));

                }));*/
                var test1 = host.New();
                test1.description = "First test";
                var test1A = host.New();
                test1A.description = "should do sth";
                test1A.doIt = new Action(() => {
                    Console.WriteLine("Performing");
                });
                test1.it1 = test1A;
                global.test1 = test1;

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}",e);
            }
            finally
            {
                host.Dispose();
            }
        }
    }
}