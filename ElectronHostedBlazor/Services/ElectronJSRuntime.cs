// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Services
{
    using System;
    using System.Text.Json;
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;
    using Microsoft.JSInterop.Infrastructure;
    using NodeHostEnvironment;

    // TODO: Proper error handling and logging
    internal sealed class ElectronJSRuntime : JSInProcessRuntime, IJSRuntime, IJSInProcessRuntime
    {
        private readonly IBridgeToNode _node;
        private readonly dynamic _jsCallDispatcher;

#if NET5_0
        public ElementReferenceContext ElementReferenceContext { get; }
#elif NETCOREAPP3_1

#endif

        public ElectronJSRuntime(IBridgeToNode node)
        {
            _node = node;
            using var dotNet = node.Global.DotNet;
            _jsCallDispatcher = dotNet.jsCallDispatcher;

            using var dotnetDispatcher = node.New();
            dotnetDispatcher.invokeDotNetFromJS = new Func<string, string, dynamic, string, string>(InvokeDotNetFromJS);
            dotnetDispatcher.beginInvokeDotNetFromJS = new Action<long, string, string, dynamic, string>(BeginInvokeDotNetFromJS);
            dotnetDispatcher.endInvokeJSFromDotNet = new Action<long, bool, string>(EndInvokeJSFromDotNet);
            dotNet.attachDispatcher(dotnetDispatcher);

#if NET5_0
        ElementReferenceContext = new WebElementReferenceContext(this);
        JsonSerializerOptions.Converters.Add(new ElementReferenceJsonConverter(ElementReferenceContext));
#elif NETCOREAPP3_1
        JsonSerializerOptions.Converters.Add(new ElementReferenceJsonConverter());
#endif
            
        }

        private void EndInvokeJSFromDotNet(long callId, bool succeeded, string argsJson)
        {
            DotNetDispatcher.EndInvokeJS(this, argsJson);
        }

        private void BeginInvokeDotNetFromJS(long callId, string assemblyName, string methodIdentifier, dynamic dotNetObjectId, string argsJson)
        {
            var callInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId == null ? default : (long)dotNetObjectId, callId.ToString());
            DotNetDispatcher.BeginInvokeDotNet(this, callInfo, argsJson);
        }

        private string InvokeDotNetFromJS(string assemblyName, string methodIdentifier, dynamic dotNetObjectId, string argsJson)
        {
            var callInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId == null ? default : (long)dotNetObjectId, null);
            return DotNetDispatcher.Invoke(this, callInfo, argsJson);
        }

        protected override void EndInvokeDotNet(DotNetInvocationInfo invocationInfo, in DotNetInvocationResult invocationResult)
        {
            if (!_node.CheckAccess())
            {
                // TODO DM 27.04.2020: This seems to happen in electron, we should investigate how this is possible as dotnet uses TaskScheduler.Current
                // Exceptions do not propagate here
                var result = invocationResult;
                _node.Run(() => _jsCallDispatcher.endInvokeDotNetFromJSWithJson(invocationInfo.CallId,
                                                                                result.Success,
                                                                                result.Success
                                                                                    ? JsonSerializer.Serialize(result.Result, JsonSerializerOptions)
                                                                                    : result.Exception.ToString()))
                     .Wait();
                return;
            }

            _jsCallDispatcher.endInvokeDotNetFromJSWithJson(invocationInfo.CallId,
                                                            invocationResult.Success,
                                                            invocationResult.Success
                                                                ? JsonSerializer.Serialize(invocationResult.Result, JsonSerializerOptions)
                                                                : invocationResult.Exception.ToString());
        }

#if NET5_0
        protected override string? InvokeJS(string identifier, string? argsJson, JSCallResultType resultType, long targetInstanceId)
        {
            // TODO: Use extra args
            if (!_node.CheckAccess())
                return _node.Run(() => _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson, (int)resultType, targetInstanceId)).Result;

            return _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson, (int)resultType, targetInstanceId);
        }

        protected override void BeginInvokeJS(long taskId, string identifier, string? argsJson, JSCallResultType resultType, long targetInstanceId)
        {
            // TODO: Use extra args
            if (!_node.CheckAccess())
            {
                // TODO DM 27.04.2020: Consider closing taskId on an exception
                _node.Run(() => _jsCallDispatcher.beginInvokeJSFromDotNet(taskId, identifier, argsJson, (int)resultType, targetInstanceId))
                     .Wait();
                return;
            }

            _jsCallDispatcher.beginInvokeJSFromDotNet(taskId, identifier, argsJson, (int)resultType, targetInstanceId);
        }


    #elif NETCOREAPP3_1
        protected override string InvokeJS(string identifier, string argsJson)
        {
            if (!_node.CheckAccess())
                return _node.Run(() => _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson, 0, 0)).Result;

            return _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson, 0, 0);
        }

        protected override void BeginInvokeJS(long taskId, string identifier, string argsJson)
        {
            if (!_node.CheckAccess())
            {
                // TODO DM 27.04.2020: Consider closing taskId on an exception
                _node.Run(() => _jsCallDispatcher.beginInvokeJSFromDotNet(taskId, identifier, argsJson, 0, 0))
                     .Wait();
                return;
            }

            _jsCallDispatcher.beginInvokeJSFromDotNet(taskId, identifier, argsJson, 0, 0);
        }

        
#endif
    }
}
