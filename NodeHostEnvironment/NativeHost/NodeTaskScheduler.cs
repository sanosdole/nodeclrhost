namespace NodeHostEnvironment.NativeHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class NodeTaskScheduler : TaskScheduler
    {
        private readonly IntPtr _context;
        private readonly Context _synchronizationContext;
        private readonly SignalEventLoopEntry _signalEventLoopEntry;
        private readonly SignalMicroTask _signalMicroTask;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable as we need it to stay alive
        private readonly ProcessJsEventLoopEntry _onProcessJsEventLoopEntry;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable as we need it to stay alive
        private readonly ProcessMicroTask _onProcessMicroTask;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable as we need it to stay alive
        private readonly ClosingRuntime _onClosingRuntime;

        private readonly Queue<Task> _microTasks = new Queue<Task>();
        private readonly ManualResetEventSlim _closedEvent = new ManualResetEventSlim(false);

        private volatile Thread _activeThread;

        public NodeTaskScheduler(IntPtr context, NativeApi nativeMethods)
        {
            _context = context;
            _signalEventLoopEntry = nativeMethods.SignalEventLoopEntry;
            _signalMicroTask = nativeMethods.SignalMicroTask;
            Factory = new TaskFactory(this);
            _synchronizationContext = new Context(Factory);

            _onProcessJsEventLoopEntry = OnProcessJSEventLoopEntry;
            _onProcessMicroTask = OnProcessMicroTask;
            _onClosingRuntime = OnClosingRuntime;
            nativeMethods.RegisterSchedulerCallbacks(context, _onProcessJsEventLoopEntry, _onProcessMicroTask, _onClosingRuntime);
        }

        public bool ContextIsActive => _activeThread == Thread.CurrentThread;

        public object RunCallbackSynchronously(Func<object, object> callback, object args)
        {
            // If we have a dotnet stack, micro task processing will happen once its done.
            if (ContextIsActive)
                return callback(args);

            // We have been called on a JS stack, start a new dotnet stack.
            // MicroTask processing should be triggered when the JS stack is empty.

            // Ensure context
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
            _activeThread = Thread.CurrentThread;
            // Ensure TaskScheduler.Current
            var task = new Task<object>(callback, args);
            task.RunSynchronously(this);
            _activeThread = null;
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
            if (_closedEvent.IsSet)
            {
                // Throwing crashes the runtime and there is no way to fail the task :(
                //throw new InvalidOperationException("The JS runtime has shut down!");
                return;
            }

            if (ContextIsActive)
            {
                // If we are on the right thread we use a micro task.
                // It will be processed in order with JS micro tasks once the stack is empty.
                // TODO DM 27.04.2020: Profile if using a private queue is more efficient than creating a new JS func on every invocation
                _microTasks.Enqueue(task);
                _signalMicroTask(_context, IntPtr.Zero);
                return;
            }

            var handle = GCHandle.Alloc(task, GCHandleType.Normal);
            var handlePtr = GCHandle.ToIntPtr(handle);

            // Otherwise it will be queued to the event loop.
            _signalEventLoopEntry(_context, handlePtr);
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
            // TODO DM 27.04.2020: Support debugger in DEBUG builds
            return Enumerable.Empty<Task>();
        }

        private void OnProcessMicroTask(IntPtr data)
        {
            /*var handle = GCHandle.FromIntPtr(data);
            var microTask = (Task)handle.Target;*/
            var microTask = _microTasks.Dequeue();

            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
            _activeThread = Thread.CurrentThread;
            TryExecuteTask(microTask);
            _activeThread = null;
            //handle.Free();
        }

        private void OnProcessJSEventLoopEntry(IntPtr data)
        {
            var handle = GCHandle.FromIntPtr(data);
            var macroTask = (Task)handle.Target;

            // We need a synchronization context to prevent the async targeting pack
            // from in-lining thread pool continuations onto the Node thread. This works as it uses
            // the synchronization context to detect whether a normal continuation
            // or a continuation with a specific TaskScheduler is used.
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
            _activeThread = Thread.CurrentThread;
            TryExecuteTask(macroTask);
            _activeThread = null;
            handle.Free();
        }

        private void OnClosingRuntime()
        {
            GC.Collect();
            // TODO: DEAD-LOCKS
            //GC.WaitForPendingFinalizers();
            _closedEvent.Set();
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
