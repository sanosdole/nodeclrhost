namespace TestApp
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using NodeHostEnvironment;

    public abstract class MochaTest
    {
        protected IBridgeToNode Node { get; private set; }
        protected dynamic Global { get; private set; }

        public void Register(IBridgeToNode node, Action afterCallback)
        {
            Node = node;
            Global = node.Global;

            Global.describe(GetType().Name.Replace("_", " "),
                            new Action(() =>
                                       {
                                           Global.after(afterCallback);
                                           foreach (var method in GetType().GetMethods())
                                           {
                                               var callback = method.ReturnType == typeof(void) ? SyncTest(method) : AsyncTest(method);

                                               if (method.Name == "Before")
                                                   Global.before(callback);
                                               else if (method.Name.StartsWith("It_"))
                                                   Global.it(method.Name.Replace("It_", " ").Replace("_", " "), callback);
                                           }
                                       }));
        }

        private Delegate SyncTest(MethodInfo method)
        {
            return new Action(() =>
                              {
                                  try
                                  {
                                      method.Invoke(this, null);
                                  }
                                  catch (TargetInvocationException tie)
                                  {
                                      throw tie.InnerException;
                                  }
                              });
        }

        private Delegate AsyncTest(MethodInfo method)
        {
            return new Func<Task>(() =>
                                  {
                                      try
                                      {
                                          return (Task)method.Invoke(this, null);
                                      }
                                      catch (AggregateException ae)
                                      {
                                          ae = ae.Flatten();
                                          if (ae.InnerExceptions.Count == 1)
                                              throw ae.InnerExceptions[0];
                                          throw;
                                      }
                                      catch (TargetInvocationException tie)
                                      {
                                          throw tie.InnerException;
                                      }
                                  });
        }
    }
}
