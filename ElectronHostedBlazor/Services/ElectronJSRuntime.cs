// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Services
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;
    using Microsoft.JSInterop.Infrastructure;
    using NodeHostEnvironment;
    using Hosting;

    internal sealed class ElectronJSRuntime : JSInProcessRuntime/*, IJSUnmarshalledRuntime*/
    {
        private readonly IBridgeToNode _node;
        private readonly dynamic _jsCallDispatcher;

        public ElementReferenceContext ElementReferenceContext { get; }

        public ElectronJSRuntime(IBridgeToNode node)
        {
            ElementReferenceContext = new WebElementReferenceContext(this);
            JsonSerializerOptions.Converters.Add(new ElementReferenceJsonConverter(ElementReferenceContext));

            // TODO: Do we need this? Would it be good to implement?
            //JsonSerializerOptions.Converters.Insert(0, new WebAssemblyJSObjectReferenceJsonConverter(this));
            _node = node;

            using var dotNet = node.Global.DotNet;
            _jsCallDispatcher = dotNet.jsCallDispatcher;

            using var dotnetDispatcher = node.New();
            dotnetDispatcher.invokeDotNetFromJS = new Func<string, string, long?, string, string?>(InvokeDotNet);
            dotnetDispatcher.beginInvokeDotNetFromJS = new Action<long, string, string, long?, string>(BeginInvokeDotNet);
            dotnetDispatcher.endInvokeJSFromDotNet = new Action<long, bool, string>(EndInvokeJS);
            dotnetDispatcher.sendByteArray = new Action<int, byte[]>(NotifyByteArrayAvailable);
            dotNet.attachDispatcher(dotnetDispatcher);
        }       

        // TODO: We need to register at JS Code for callbacks
        // The following methods are invoke via Mono's JS interop mechanism (invoke_method)
        private string? InvokeDotNet(string assemblyName, string methodIdentifier, long? dotNetObjectId, string argsJson)
        {
            var callInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId == null ? default : dotNetObjectId.Value, callId: null);
            return DotNetDispatcher.Invoke(this, callInfo, argsJson);
        }

        /// <summary>
        /// Invoked via Mono's JS interop mechanism (invoke_method)
        ///
        /// Notifies .NET of an array that's available for transfer from JS to .NET
        ///
        /// Ideally that byte array would be transferred directly as a parameter on this
        /// call, however that's not currently possible due to: https://github.com/dotnet/runtime/issues/53378
        /// </summary>
        /// <param name="id">Id of the byte array</param>
        private void NotifyByteArrayAvailable(int id, byte[] data)
        {
            //var data = Instance.InvokeUnmarshalled<byte[]>("Blazor._internal.retrieveByteArray");
            DotNetDispatcher.ReceiveByteArray(this, id, data);
        }

        // Invoked via Mono's JS interop mechanism (invoke_method)
        private void EndInvokeJS(long asyncHandle, bool succeeded, string argsJson)
        {
            ElectronCallQueue.Schedule((thiz: this, argsJson), static state =>
            {
                // This is not expected to throw, as it takes care of converting any unhandled user code
                // exceptions into a failure on the Task that was returned when calling InvokeAsync.
                DotNetDispatcher.EndInvokeJS(state.thiz, state.argsJson);
            });
        }

        // Invoked via Mono's JS interop mechanism (invoke_method)
        private void BeginInvokeDotNet(long callId, string assemblyName, string methodIdentifier, long? dotNetObjectId, string argsJson)
        {
            /*// Figure out whether 'assemblyNameOrDotNetObjectId' is the assembly name or the instance ID
            // We only need one for any given call. This helps to work around the limitation that we can
            // only pass a maximum of 4 args in a call from JS to Mono Electron.
            string? assemblyName;
            long dotNetObjectId;
            if (char.IsDigit(assemblyNameOrDotNetObjectId[0]))
            {
                dotNetObjectId = long.Parse(assemblyNameOrDotNetObjectId, CultureInfo.InvariantCulture);
                assemblyName = null;
            }
            else
            {
                dotNetObjectId = default;
                assemblyName = assemblyNameOrDotNetObjectId;
            }*/

            var callInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId == null ? default : dotNetObjectId.Value, callId.ToString());
            ElectronCallQueue.Schedule((thiz: this, callInfo, argsJson), static state =>
            {
                // This is not expected to throw, as it takes care of converting any unhandled user code
                // exceptions into a failure on the JS Promise object.
                DotNetDispatcher.BeginInvokeDotNet(state.thiz, state.callInfo, state.argsJson);
            });
        }

        public JsonSerializerOptions ReadJsonSerializerOptions() => JsonSerializerOptions;

        /// <inheritdoc />
        protected override string InvokeJS(string identifier, string? argsJson, JSCallResultType resultType, long targetInstanceId)
        {
            /*var callInfo = new JSCallInfo
            {
                FunctionIdentifier = identifier,
                TargetInstanceId = targetInstanceId,
                ResultType = resultType,
                MarshalledCallArgsJson = argsJson ?? "[]",
                MarshalledCallAsyncHandle = default
            };

            var result = InternalCalls.InvokeJS<object, object, object, string>(out var exception, ref callInfo, null, null, null);

            return exception != null
                ? throw new JSException(exception)
                : result;*/
                // TODO: Use extra args
            if (!_node.CheckAccess())
                return _node.Run(() => _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson, (int) resultType, targetInstanceId)).Result;

            return _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson, (int) resultType, targetInstanceId);
        }

        /// <inheritdoc />
        protected override void BeginInvokeJS(long asyncHandle, string identifier, string? argsJson, JSCallResultType resultType, long targetInstanceId)
        {
            /*var callInfo = new JSCallInfo
            {
                FunctionIdentifier = identifier,
                TargetInstanceId = targetInstanceId,
                ResultType = resultType,
                MarshalledCallArgsJson = argsJson ?? "[]",
                MarshalledCallAsyncHandle = asyncHandle
            };

            InternalCalls.InvokeJS<object, object, object, string>(out _, ref callInfo, null, null, null);*/
            // TODO: Use extra args
            if (!_node.CheckAccess())
            {
                // TODO DM 27.04.2020: Consider closing taskId on an exception
                _node.Run(() => _jsCallDispatcher.beginInvokeJSFromDotNet(asyncHandle, identifier, argsJson, (int) resultType, targetInstanceId))
                    .Wait();
                return;
            }

            _jsCallDispatcher.beginInvokeJSFromDotNet(asyncHandle, identifier, argsJson, (int) resultType, targetInstanceId);
        }

        /// <inheritdoc />
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "TODO: This should be in the xml suppressions file, but can't be because https://github.com/mono/linker/issues/2006")]
        protected override void EndInvokeDotNet(DotNetInvocationInfo callInfo, in DotNetInvocationResult dispatchResult)
        {
            /*var resultJsonOrErrorMessage = dispatchResult.Success
                ? dispatchResult.ResultJson!
                : dispatchResult.Exception!.ToString();
            InvokeUnmarshalled<string?, bool, string, object>("Blazor._internal.endInvokeDotNetFromJS",
                callInfo.CallId, dispatchResult.Success, resultJsonOrErrorMessage);*/
                if (!_node.CheckAccess())
            {
                // TODO DM 27.04.2020: This seems to happen in electron, we should investigate how this is possible as dotnet uses TaskScheduler.Current
                // Exceptions do not propagate here
                var result = dispatchResult;
                _node.Run(() => _jsCallDispatcher.endInvokeDotNetFromJSWithJson(callInfo.CallId,
                        result.Success,
                        result.Success ?
                        JsonSerializer.Serialize(
                            result.ResultJson,
                            JsonSerializerOptions) :
                        result.Exception.ToString()))
                    .Wait();
                return;
            }

            _jsCallDispatcher.endInvokeDotNetFromJSWithJson(callInfo.CallId,
                dispatchResult.Success,
                dispatchResult.Success ?
                JsonSerializer.Serialize(
                    dispatchResult.ResultJson,
                    JsonSerializerOptions) :
                dispatchResult.Exception.ToString());
        }

        /// <inheritdoc />
        protected override void SendByteArray(int id, byte[] data)
        {
            _jsCallDispatcher.receiveByteArray(id, data);
            //InvokeUnmarshalled<int, byte[], object>("Blazor._internal.receiveByteArray", id, data);
        }

         [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "IJSStreamReference is referenced in Microsoft.JSInterop.Infrastructure.JSStreamReferenceJsonConverter")]
        private IJSStreamReference DeserializeJSStreamReference(string serializedStreamReference)
        {
            var jsStreamReference = JsonSerializer.Deserialize<IJSStreamReference>(serializedStreamReference, JsonSerializerOptions);
            if (jsStreamReference is null)
            {
                throw new NullReferenceException($"Unable to parse the {nameof(serializedStreamReference)}.");
            }

            return jsStreamReference;
        }

        /*private TResult InvokeUnmarshalled<T0, T1, T2, TResult>(string identifier, T0 arg0, T1 arg1, T2 arg2, long targetInstanceId)
        {
            var parts = identifier.Split('.');
            var o = _node.Global;
            for (int c = 0; c < parts.Length; c++)
            {
                o = o[parts[c]];
            }
            return ((Func<T0, T1, T2, TResult>)o)(arg0, arg1, arg2);
            var resultType = JSCallResultTypeHelper.FromGeneric<TResult>();

            var callInfo = new JSCallInfo
            {
                FunctionIdentifier = identifier,
                TargetInstanceId = targetInstanceId,
                ResultType = resultType,
            };

            string exception;

            switch (resultType)
            {
                case JSCallResultType.Default:
                case JSCallResultType.JSVoidResult:
                    var result = InternalCalls.InvokeJS<T0, T1, T2, TResult>(out exception, ref callInfo, arg0, arg1, arg2);
                    return exception != null
                        ? throw new JSException(exception)
                        : result;
                case JSCallResultType.JSObjectReference:
                    var id = InternalCalls.InvokeJS<T0, T1, T2, int>(out exception, ref callInfo, arg0, arg1, arg2);
                    return exception != null
                        ? throw new JSException(exception)
                        : (TResult)(object)new WebAssemblyJSObjectReference(this, id);
                case JSCallResultType.JSStreamReference:
                    var serializedStreamReference = InternalCalls.InvokeJS<T0, T1, T2, string>(out exception, ref callInfo, arg0, arg1, arg2);
                    return exception != null
                        ? throw new JSException(exception)
                        : (TResult)(object)DeserializeJSStreamReference(serializedStreamReference);
                default:
                    throw new InvalidOperationException($"Invalid result type '{resultType}'.");
            }
        }

        /// <inheritdoc />
        public TResult InvokeUnmarshalled<TResult>(string identifier)
            => InvokeUnmarshalled<object?, object?, object?, TResult>(identifier, null, null, null, 0);

        /// <inheritdoc />
        public TResult InvokeUnmarshalled<T0, TResult>(string identifier, T0 arg0)
            => InvokeUnmarshalled<T0, object?, object?, TResult>(identifier, arg0, null, null, 0);

        /// <inheritdoc />
        public TResult InvokeUnmarshalled<T0, T1, TResult>(string identifier, T0 arg0, T1 arg1)
            => InvokeUnmarshalled<T0, T1, object?, TResult>(identifier, arg0, arg1, null, 0);

        /// <inheritdoc />
        public TResult InvokeUnmarshalled<T0, T1, T2, TResult>(string identifier, T0 arg0, T1 arg1, T2 arg2)
            => InvokeUnmarshalled<T0, T1, T2, TResult>(identifier, arg0, arg1, arg2, 0);
*/

        /// <inheritdoc />
        protected override Task<Stream> ReadJSDataAsStreamAsync(IJSStreamReference jsStreamReference, long totalLength, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(PullFromJSDataStream.CreateJSDataStream(this, jsStreamReference, totalLength, cancellationToken));

        /// <inheritdoc />
        protected override Task TransmitStreamAsync(long streamId, DotNetStreamReference dotNetStreamReference)
        {
            return TransmitDataStreamToJS.TransmitStreamAsync(this, streamId, dotNetStreamReference);
        }
    }
}
