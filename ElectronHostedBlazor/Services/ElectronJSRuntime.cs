// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using NodeHostEnvironment;

namespace ElectronHostedBlazor.Services
{
    // TODO: Proper error handling and logging
    internal sealed class ElectronJSRuntime : JSInProcessRuntime, IJSRuntime, IJSInProcessRuntime
    {
        private readonly IBridgeToNode _node;
        private readonly dynamic _window;
        private readonly dynamic _dotNet;
        private readonly dynamic _jsCallDispatcher;

        public ElectronJSRuntime(IBridgeToNode node)
        {
            _node = node;
            _window = node.Global.window;
            _dotNet = node.Global.DotNet; // No need for require, as electron-blazor-glue exports this. 
            _jsCallDispatcher = _dotNet.jsCallDispatcher;

            var dotnetDispatcher = node.New();
            dotnetDispatcher.invokeDotNetFromJS = new Func<string, string, dynamic, string, string>(InvokeDotNetFromJS);
            dotnetDispatcher.beginInvokeDotNetFromJS = new Action<long, string, string, dynamic, string>(BeginInvokeDotNetFromJS);
            dotnetDispatcher.endInvokeJSFromDotNet = new Action<long, bool, string>(EndInvokeJSFromDotNet);
            _dotNet.attachDispatcher(dotnetDispatcher);
            JsonSerializerOptions.Converters.Add(new ElementReferenceJsonConverter());
        }

        private void EndInvokeJSFromDotNet(long callId, bool succeeded, string argsJson)
        {
            DotNetDispatcher.EndInvokeJS(this, argsJson);
        }

        private void BeginInvokeDotNetFromJS(long callId, string assemblyName, string methodIdentifier, dynamic dotNetObjectId, string argsJson)
        {
            var callInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId == null ? default : (long) dotNetObjectId, callId.ToString());
            DotNetDispatcher.BeginInvokeDotNet(this, callInfo, argsJson);
        }

        private string InvokeDotNetFromJS(string assemblyName, string methodIdentifier, dynamic dotNetObjectId, string argsJson)
        {
            var callInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId == null ? default : (long) dotNetObjectId, null);
            return DotNetDispatcher.Invoke(this, callInfo, argsJson);
        }

        protected override string InvokeJS(string identifier, string argsJson)
        {
            if (!_node.CheckAccess())
                return _node.Run(() => _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson)).Result;

            return _jsCallDispatcher.invokeJSFromDotNet(identifier, argsJson);
        }

        protected override void BeginInvokeJS(long taskId, string identifier, string argsJson)
        {
            if (!_node.CheckAccess())
            {
                // Wait is for propagating exceptions
                _node.Run(() => _jsCallDispatcher.beginInvokeJSFromDotNet(taskId, identifier, argsJson)).Wait();
                return;
            }

            _jsCallDispatcher.beginInvokeJSFromDotNet(taskId, identifier, argsJson);
        }

        protected override void EndInvokeDotNet(DotNetInvocationInfo invocationInfo, in DotNetInvocationResult invocationResult)
        {
            if (!_node.CheckAccess())
                throw new InvalidOperationException("DotNetDispatcher did not use the proper TaskScheduler for its continuation!");

            _jsCallDispatcher.endInvokeDotNetFromJS(invocationInfo.CallId,
                invocationResult.Success,
                invocationResult.Success ?
                JsonSerializer.Serialize(invocationResult.Result, JsonSerializerOptions) :
                invocationResult.Exception.ToString());
        }
    }
}