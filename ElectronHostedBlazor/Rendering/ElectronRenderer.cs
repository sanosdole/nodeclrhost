// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.RenderTree;
    using Microsoft.Extensions.Logging;
    using NodeHostEnvironment;

    /// <summary>
    /// Provides mechanisms for rendering <see cref="IComponent"/> instances in a
    /// web browser, dispatching events to them, and refreshing the UI as required.
    /// </summary>
    internal class ElectronRenderer : Renderer
    {
        private bool _isDispatchingEvent;
        private readonly Queue<IncomingEventInfo> _deferredIncomingEvents = new Queue<IncomingEventInfo>();
        private readonly ILogger<ElectronRenderer> _logger;
        private readonly dynamic _blazorInternal;
        private readonly ElectronDispatcher _dispatcher;
        private readonly dynamic _blazorInternalRenderBatch;
        private readonly ReusableArrayBufferStream _reusableArrayBufferStream;
        private readonly IBridgeToNode _node;

        /// <summary>
        /// Constructs an instance of <see cref="ElectronRenderer"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use when initializing components.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="node">The bridge to use for JS interop</param>
        public ElectronRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IBridgeToNode node) : base(serviceProvider, loggerFactory)
        {
            _node = node;
            _logger = loggerFactory.CreateLogger<ElectronRenderer>();
            _reusableArrayBufferStream = new ReusableArrayBufferStream(node);
            _blazorInternal = node.Global.window.Blazor._internal;
            _dispatcher = new ElectronDispatcher(node);
            var eventDispatcher = new ElectronEventDispatcher(this);
            _blazorInternal.HandleRendererEvent = new Func<dynamic, string, Task>(eventDispatcher.DispatchEvent);
            _blazorInternalRenderBatch = _blazorInternal.renderBatch;
        }

        public override Dispatcher Dispatcher => _dispatcher;

        /// <summary>
        /// Attaches a new root component to the renderer,
        /// causing it to be displayed in the specified DOM element.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component.</typeparam>
        /// <param name="domElementSelector">A CSS selector that uniquely identifies a DOM element.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous rendering of the added component.</returns>
        /// <remarks>
        /// Callers of this method may choose to ignore the returned <see cref="Task"/> if they do not
        /// want to await the rendering of the added component.
        /// </remarks>
        public Task AddComponentAsync<TComponent>(string domElementSelector)
            where TComponent : IComponent => AddComponentAsync(typeof(TComponent), domElementSelector);

        /// <summary>
        /// Associates the <see cref="IComponent"/> with the <see cref="ElectronRenderer"/>,
        /// causing it to be displayed in the specified DOM element.
        /// </summary>
        /// <param name="componentType">The type of the component.</param>
        /// <param name="domElementSelector">A CSS selector that uniquely identifies a DOM element.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous rendering of the added component.</returns>
        /// <remarks>
        /// Callers of this method may choose to ignore the returned <see cref="Task"/> if they do not
        /// want to await the rendering of the added component.
        /// </remarks>
        public Task AddComponentAsync(Type componentType, string domElementSelector)
        {
            var component = InstantiateComponent(componentType);
            var componentId = AssignRootComponentId(component);

            // The only reason we're calling this synchronously is so that, if it throws,
            // we get the exception back *before* attempting the first UpdateDisplayAsync
            // (otherwise the logged exception will come from UpdateDisplayAsync instead of here)
            // When implementing support for out-of-process runtimes, we'll need to call this
            // asynchronously and ensure we surface any exceptions correctly.

            /*NodeJSRuntime.Instance.Invoke<object>(
                "Blazor._internal.attachRootComponentToElement",
                domElementSelector,
                componentId,
                _NodeRendererId); */
            _blazorInternal.attachRootComponentToElement(domElementSelector, componentId, /*_NodeRendererId*/ 1);

            return RenderRootComponentAsync(componentId);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _blazorInternal.HandleRendererEvent = null;
        }

        /// <inheritdoc />
        protected override Task UpdateDisplayAsync(in RenderBatch batch)
        {
            // TODO DM 22.08.2019: Using out of process render batch is inefficient
            //var arrayBuilder = new ArrayBuilder<byte>(2048);

            _reusableArrayBufferStream.SetLength(0);
            _reusableArrayBufferStream.Position = 0;
            using (var writer = new RenderBatchWriter(_reusableArrayBufferStream, true))
                writer.Write(batch);

            // Prevent event dispatching while updating the DOM
            var wasDispatchingEvent = _isDispatchingEvent;
            _isDispatchingEvent = true;

            try
            {
                _blazorInternalRenderBatch( /*_NodeRendererId*/ 1, _reusableArrayBufferStream.Buffer.JsObject, _reusableArrayBufferStream.Length);
            }
            finally
            {
                if (!wasDispatchingEvent)
                {
                    _isDispatchingEvent = false;

                    if (_deferredIncomingEvents.Count > 0)
                    {
                        // Fire-and-forget because the task we return from this method should only reflect the
                        // completion of its own event dispatch, not that of any others that happen to be queued.
                        // Also, ProcessNextDeferredEventAsync deals with its own async errors.
                        _ = ProcessNextDeferredEventAsync();
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override void HandleException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                {
                    _logger.LogError(innerException, "Unhandled exception while rendering a component");
                }
            }
            else
            {
                _logger.LogError(exception, "Unhandled exception while rendering a component");
            }

            _dispatcher.PublishRendererException(exception);
        }

        private bool _pendingRenderQueued;

        /// <summary>
        /// Processses pending renders requests from components if there are any.
        /// </summary>
        protected override void ProcessPendingRender()
        {
            if (_pendingRenderQueued)
                return;
            _pendingRenderQueued = true;
            // The node scheduler will schedule this as micro task.
            // So we queue up all rendering until the current stack is empty.
            // In combination with the StateHasChanged implementation in ComponentBase this will prevent unnecessary
            // renderings from components that call StateHasChanged in fast succession synchronously.
            // Sadly the task has to be observed by the GC...
            _node.Run(Execute);

            void Execute()
            {
                _pendingRenderQueued = false;
                base.ProcessPendingRender();
            }
        }

        /// <inheritdoc />
        public override Task DispatchEventAsync(ulong eventHandlerId, EventFieldInfo eventFieldInfo, EventArgs eventArgs)
        {
            // Be sure we only run one event handler at once. Although they couldn't run
            // simultaneously anyway (there's only one thread), they could run nested on
            // the stack if somehow one event handler triggers another event synchronously.
            // We need event handlers not to overlap because (a) that's consistent with
            // server-side Blazor which uses a sync context, and (b) the rendering logic
            // relies completely on the idea that within a given scope it's only building
            // or processing one batch at a time.
            //
            // The only currently known case where this makes a difference is in the E2E
            // tests in ReorderingFocusComponent, where we hit what seems like a Chrome bug
            // where mutating the DOM cause an element's "change" to fire while its "input"
            // handler is still running (i.e., nested on the stack) -- this doesn't happen
            // in Firefox. Possibly a future version of Chrome may fix this, but even then,
            // it's conceivable that DOM mutation events could trigger this too.

            if (_isDispatchingEvent)
            {
                var info = new IncomingEventInfo(eventHandlerId, eventFieldInfo, eventArgs);
                _deferredIncomingEvents.Enqueue(info);
                return info.TaskCompletionSource.Task;
            }
            else
            {
                try
                {
                    _isDispatchingEvent = true;
                    return base.DispatchEventAsync(eventHandlerId, eventFieldInfo, eventArgs);
                }
                finally
                {
                    _isDispatchingEvent = false;

                    if (_deferredIncomingEvents.Count > 0)
                    {
                        // Fire-and-forget because the task we return from this method should only reflect the
                        // completion of its own event dispatch, not that of any others that happen to be queued.
                        // Also, ProcessNextDeferredEventAsync deals with its own async errors.
                        _ = ProcessNextDeferredEventAsync();
                    }
                }
            }
        }

        private async Task ProcessNextDeferredEventAsync()
        {
            var info = _deferredIncomingEvents.Dequeue();
            var taskCompletionSource = info.TaskCompletionSource;

            try
            {
                await DispatchEventAsync(info.EventHandlerId, info.EventFieldInfo, info.EventArgs);
                taskCompletionSource.SetResult(null);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        }

        readonly struct IncomingEventInfo
        {
            public readonly ulong EventHandlerId;
            public readonly EventFieldInfo EventFieldInfo;
            public readonly EventArgs EventArgs;
            public readonly TaskCompletionSource<object> TaskCompletionSource;

            public IncomingEventInfo(ulong eventHandlerId, EventFieldInfo eventFieldInfo, EventArgs eventArgs)
            {
                EventHandlerId = eventHandlerId;
                EventFieldInfo = eventFieldInfo;
                EventArgs = eventArgs;
                TaskCompletionSource = new TaskCompletionSource<object>();
            }
        }
    }
}
