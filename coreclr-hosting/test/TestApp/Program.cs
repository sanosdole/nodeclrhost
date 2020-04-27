namespace TestApp
{
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   using NodeHostEnvironment;
   using Tests;

   public class Program
   {
      public static Task<int> Main(string[] args)
      {
         var mainCtx = SynchronizationContext.Current;
         var mainScheduler = TaskScheduler.Current;
         var host = NodeHost.Instance;
         var tcs = new TaskCompletionSource<int>();

         /*Console.WriteLine("Waiting for debugger!");
         while(!System.Diagnostics.Debugger.IsAttached) System.Threading.Thread.Sleep(50);*/
         //System.Diagnostics.Debugger.Launch();

         host.Global.registerAsyncTest(new Func<Task>(() => Task.Delay(5)));

         var tests = new MochaTest[]
                     {
                        new Arguments(args),
                        new Global_test_object(),
                        new Task_scheduler(mainCtx, mainScheduler)
                     };

         var remainingTestCount = tests.Length;

         // Important: As mocha runs the tests asynchronously, we have to dispose after all tests have been run.
         //            But global after will never be called if we are waiting for the return value of this method
         var afterCallback = new Action(() =>
                                        {
                                           remainingTestCount--;
                                           if (remainingTestCount <= 0)
                                           {
                                              tcs.SetResult(0);
                                           }
                                        });

         foreach (var test in tests)
            test.Register(host, afterCallback);

         return tcs.Task;
      }
   }
}
