namespace Benchmark
{
   using System;
   using System.Threading;
   using System.Threading.Tasks;
   using NodeHostEnvironment;

   public class Program
   {
      public static Task<int> Main(string[] args)
      {
         var host = NodeHost.Instance;
         var tcs = new TaskCompletionSource<int>();
         host.Global.closeDotNet = new Action(() => tcs.SetResult(0));

         var cbVoidToVoid = new Action(() => {});         
         host.Global.dotnetCallbacks.cbVoidToVoid = cbVoidToVoid;         
         host.Global.dotnetCallbacks.cbIntIntToInt = new Func<int,int,int>((a,b) => a + b);
         host.Global.dotnetCallbacks.cbIntIntToInt(0,0);
         host.Global.dotnetCallbacks.cbVoidToTask = new Func<Task>(() => Task.CompletedTask);
         host.Global.dotnetCallbacks.cbIntToTaskDelay = new Func<int, Task>(delay => Task.Delay(delay));
         host.Global.dotnetCallbacks.cbIntToTaskDelayAsync = new Func<int, Task>(async delay => await Task.Delay(delay));
         host.Global.dotnetCallbacks.cbVoidToTaskYield = new Func<Task>(async () => await Task.Yield());
         host.Global.dotnetCallbacks.cbVoidToTaskYieldPromise = new Func<Task>(() => (Task)host.Global.Promise.resolve().then(new Action(() => {})));
         host.Global.dotnetCallbacks.cbVoidToTaskYieldPromiseAsync = new Func<Task>(async () => await ((Task)host.Global.Promise.resolve()));
         host.Global.dotnetCallbacks.cbTaskToTask = new Func<Task,Task>(async t => await t);
         host.Global.dotnetCallbacks.cbStringArrayToString = new Func<string[],string>(array => string.Join("|", array));
         
         return tcs.Task;
      }
   }
}
