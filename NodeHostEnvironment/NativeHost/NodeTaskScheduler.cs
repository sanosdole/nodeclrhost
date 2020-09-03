namespace NodeHostEnvironment.NativeHost
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Linq;
   using System.Runtime.InteropServices;
   using System.Threading;
   using System.Threading.Tasks;

   internal sealed class NodeTaskScheduler : TaskScheduler
   {
      private readonly NativeContext _nativeContext;
      private readonly Context _synchronizationContext;

      // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable as we need it to stay alive
      private readonly ProcessJsEventLoopEntry _onProcessJsEventLoopEntry;

      // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable as we need it to stay alive
      private readonly ProcessMicroTask _onProcessMicroTask;
      private readonly ThreadLocal<bool> _isActive = new ThreadLocal<bool>();

      private readonly Queue<Task> _microTasks = new Queue<Task>();
      private readonly HashSet<Task> _eventLoopTasks = new HashSet<Task>();
      private volatile int _stopped;

      public NodeTaskScheduler(NativeContext nativeContext)
      {
         _nativeContext = nativeContext;
         Factory = new TaskFactory(this);
         _synchronizationContext = new Context(Factory);

         _onProcessJsEventLoopEntry = OnProcessJSEventLoopEntry;
         _onProcessMicroTask = OnProcessMicroTask;
         _nativeContext.RegisterSchedulerCallbacks(_onProcessJsEventLoopEntry, _onProcessMicroTask);
      }

      public bool ContextIsActive => _isActive.Value;

      public object RunCallbackSynchronously(Func<object, object> callback, object args)
      {
         // If we have a dotnet stack, micro task processing will happen once its done.
         if (ContextIsActive)
            return callback(args);

         // We have been called on a JS stack, start a new dotnet stack.
         // MicroTask processing should be triggered when the JS stack is empty.

         // Ensure context
         SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
         _isActive.Value = true;
         // Ensure TaskScheduler.Current
         var task = new Task<object>(callback, args);
         task.RunSynchronously(this);
         _isActive.Value = false;
         return task.Result;
      }

      /// <summary>
      /// Access the <see cref="TaskFactory"/> for creating tasks on this scheduler.
      /// </summary>
      public TaskFactory Factory { get; }

      /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
      public override int MaximumConcurrencyLevel => 1;

      /// <summary>Queues a Task to be executed by this scheduler.</summary>
      /// <param name="task">The task to be executed.</param>
      protected override void QueueTask(Task task)
      {
         if (_stopped != 0)
         {
            // TODO DM 02.09.2020: Is it better to try execution?
            throw new InvalidOperationException("Scheduler has been stopped!");
         }

         if (ContextIsActive)
         {
            // If we are on the right thread we use a micro task.
            // It will be processed in order with JS micro tasks once the stack is empty.
            // TODO DM 27.04.2020: Profile if using a private queue is more efficient than creating a new JS func on every invocation
            _microTasks.Enqueue(task);
            _nativeContext.SignalMicroTask(IntPtr.Zero);
            return;
         }

         var handle = GCHandle.Alloc(task, GCHandleType.Normal);
         var handlePtr = GCHandle.ToIntPtr(handle);
         lock (_eventLoopTasks)
            _eventLoopTasks.Add(task);

         // Otherwise it will be queued to the event loop.
         _nativeContext.SignalEventLoopEntry(handlePtr);
      }

      /// <summary>
      /// Determines whether a Task may be in-lined.
      /// </summary>
      /// <param name="task">The task to be executed.</param>
      /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
      /// <returns>True if the task was successfully in-lined, otherwise false.</returns>
      protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
      {
         if (!ContextIsActive)
            return false;

         // Try to inline if the current thread is our thread so no deadlocks happen.
         // We do not remove the task from its "queue", as its TryExecute will return false if it was already executed.
         return TryExecuteTask(task);
      }

      /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
      /// <returns>An enumerable of all tasks currently scheduled.</returns>
      protected override IEnumerable<Task> GetScheduledTasks()
      {
         // TODO DM 02.09.2020: Add micro tasks for debugger support or skip debugger support and use a counter for queue size!
         lock (_eventLoopTasks)
            return _eventLoopTasks.ToList();
      }

      private void OnProcessMicroTask(IntPtr data)
      {
         var microTask = _microTasks.Dequeue();

         SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
         _isActive.Value = true;
         TryExecuteTask(microTask);
         _isActive.Value = false;
      }

      private void OnProcessJSEventLoopEntry(IntPtr data)
      {
         var handle = GCHandle.FromIntPtr(data);
         var macroTask = (Task)handle.Target;
         Debug.Assert(macroTask != null);

         // We need a synchronization context to prevent the async targeting pack
         // from in-lining thread pool continuations onto the Node thread. This works as it uses
         // the synchronization context to detect whether a normal continuation
         // or a continuation with a specific TaskScheduler is used.
         SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
         _isActive.Value = true;

         TryExecuteTask(macroTask);
         _isActive.Value = false;
         handle.Free();
         lock (_eventLoopTasks)
            _eventLoopTasks.Remove(macroTask);
      }

      private sealed class Context : SynchronizationContext
      {
         private readonly TaskFactory _factory;

         public Context(TaskFactory factory)
         {
            _factory = factory;
         }

         /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
         /// <param name="callback">The System.Threading.SendOrPostCallback delegate to call.</param>
         /// <param name="state">The object passed to the delegate.</param>
         public override void Post(SendOrPostCallback callback, object state)
         {
            if (callback == null)
               throw new ArgumentNullException(nameof(callback));
            _factory.StartNew(s => callback(s), state); // <= We just leave the task to the GC as we are only called by the TPL implementation, which catches the exceptions
         }

         /// <summary>Not supported.</summary>
         public override void Send(SendOrPostCallback callback, object state)
         {
            throw new NotSupportedException("Synchronously sending is not supported.");
         }

         public override SynchronizationContext CreateCopy()
         {
            // DM 28.o6.2o16: This ensures that the ATP does not drop the context!
            return this;
         }
      }

      public void StopAndWaitTillEmpty(Action onStopped)
      {
         async void WaitTillEmpty()
         {
            // TODO DM 02.09.2020: Is this really necessary?
            // Attempt to remove as many dynamic objects as possible before disposing the context.
            // This should minimize unreleasable object references to JS.
            // Do it asynchronously as finalizers may require to access the scheduler!
            await Task.Run(() =>
                           {
                              GC.Collect();
                              GC.WaitForPendingFinalizers(); // This could schedule tasks to _eventLoopTasks
                           });

            lock (_eventLoopTasks)
            {
               if (_eventLoopTasks.Count < 2)
                  _stopped = 1;
            }

            if (_stopped != 0)
            {
               onStopped();
               return;
            }

#pragma warning disable 4014 // As we do not want to add another continuation to the scheduler
            Factory.StartNew(WaitTillEmpty);
#pragma warning restore 4014
         }

         Factory.StartNew(WaitTillEmpty);

         while (_stopped == 0)
         { }
      }
   }
}
