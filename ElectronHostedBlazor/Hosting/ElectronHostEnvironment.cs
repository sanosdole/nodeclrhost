// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    internal sealed class ElectronHostEnvironment : IElectronHostEnvironment
    {
        public ElectronHostEnvironment(string environment, string baseAddress)
        {
            Environment = environment;
            BaseAddress = baseAddress;
        }

        public string Environment { get; }

        public string BaseAddress { get; }
    }
}
