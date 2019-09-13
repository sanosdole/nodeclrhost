namespace NodeHostEnvironment.NativeHost
{
   using System.Collections.Concurrent;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Runtime.InteropServices;
   using System.Threading.Tasks;
   using System.Threading;
   using System;

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
   internal delegate void NodeCallback(IntPtr data);

   internal sealed class NodeTaskScheduler : TaskScheduler
   {
      private const int NoPendingCallback = 0;
      private const int PendingCallback = 1;
      private int _pendingCallback;
      private readonly ConcurrentQueue<Task> _tasks;
      private readonly NodeCallback _nodeCallback;
      private readonly Context _synchronizationContext;
      private readonly Action<NodeCallback, IntPtr> _postFunc;

      public NodeTaskScheduler(Action<NodeCallback, IntPtr> postFunc)
      {
         _postFunc = postFunc;
         _nodeCallback = NodeCallback;
         _tasks = new ConcurrentQueue<Task>();
         Factory = new TaskFactory(this);
         _synchronizationContext = new Context(Factory);
         // Set immediatly as this must only be called from node main
         SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
      }

      public bool ContextIsActive => SynchronizationContext.Current == _synchronizationContext;

      public IDisposable SetNodeContext()
      {
         return new SynchronizationContextOverride(_synchronizationContext);
      }

      private sealed class SynchronizationContextOverride : IDisposable
      {
         private readonly SynchronizationContext _context;
         private readonly SynchronizationContext _previous;
         public SynchronizationContextOverride(SynchronizationContext context)
         {
            _context = context;
            _previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(_context);
         }
         public void Dispose()
         {
            Debug.Assert(SynchronizationContext.Current == _context, "Someone modified the context on this thread, this is evil!");
            SynchronizationContext.SetSynchronizationContext(_previous);

         }
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
         // this is stupid, as we never inline tasks that have not been queued!!!
         //if (TryExecuteTaskInline(task, false))
         //   return;

         // Push it into the blocking collection of tasks
         _tasks.Enqueue(task);
         if (Interlocked.Exchange(ref _pendingCallback, PendingCallback) == NoPendingCallback)
            _postFunc(_nodeCallback, IntPtr.Zero);
      }

      /// <summary>
      /// Determines whether a Task may be in-lined.
      /// </summary>
      /// <param name="task">The task to be executed.</param>
      /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
      /// <returns>True if the task was successfully in-lined, otherwise false.</returns>
      protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
      {
         // Try to inline if the current thread is our thread
         return ContextIsActive && TryExecuteTask(task);
      }

      /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
      /// <returns>An enumerable of all tasks currently scheduled.</returns>
      protected override IEnumerable<Task> GetScheduledTasks()
      {
         // Serialize the contents of the queue of tasks for the debugger
         return _tasks.ToArray();
      }

      private void NodeCallback(IntPtr data)
      {
         Interlocked.Exchange(ref _pendingCallback, NoPendingCallback);

         // We need a synchronization context to prevent the async targeting pack
         // from in-lining thread pool continuations onto the Node thread. This works as it uses
         // the synchronization context to detect whether a normal continuation
         // or a continuation with a specific TaskScheduler is used.
         using(SetNodeContext())
         {
            // Continually get the next task and try to execute it.
            // This will continue until no more tasks remain.
            Task result;
            while (_tasks.TryDequeue(out result))
               TryExecuteTask(result);
         }
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
   }
}