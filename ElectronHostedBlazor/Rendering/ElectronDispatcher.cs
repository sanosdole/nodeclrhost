// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NodeHostEnvironment;

namespace ElectronHostedBlazor.Rendering
{
    internal sealed class ElectronDispatcher : Dispatcher
    {
        private readonly IBridgeToNode _node;

        public ElectronDispatcher(IBridgeToNode node)
        {
            _node = node;
        }

        public override bool CheckAccess() => _node.CheckAccess();

        public override Task InvokeAsync(Action workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (_node.CheckAccess())
            {
                workItem();
                return Task.CompletedTask;
            }

            return _node.Run(workItem);
        }

        public override Task InvokeAsync(Func<Task> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (_node.CheckAccess())
            {
                workItem();
                return Task.CompletedTask;
            }

            return _node.RunAsync(workItem);
        }

        public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (_node.CheckAccess())
                return Task.FromResult(workItem());

            return _node.Run(workItem);
        }

        public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (_node.CheckAccess())
                return workItem();

            return _node.RunAsync(workItem);
        }
    }
}