// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using NodeHostEnvironment;

namespace ElectronHostedBlazor.Services
{
    internal sealed class ElectronNavigationInterception : INavigationInterception
    {
        private readonly dynamic _navigationManager;

        public ElectronNavigationInterception(IBridgeToNode node)
        {
            _navigationManager = node.Global.window.Blazor._internal.navigationManager;
        }        

        public Task EnableNavigationInterceptionAsync()
        {
            //NodeJSRuntime.Instance.Invoke<object>(Interop.EnableNavigationInterception);
            _navigationManager.enableNavigationInterception();
            return Task.CompletedTask;
        }
    }
}