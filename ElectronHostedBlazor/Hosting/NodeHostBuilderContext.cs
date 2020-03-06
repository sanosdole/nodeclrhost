// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

using System.Collections.Generic;

namespace ElectronHostedBlazor.Hosting
{
    public sealed class NodeHostBuilderContext
    {
        /// <summary>
        /// Creates a new <see cref="NodeHostBuilderContext" />.
        /// </summary>
        /// <param name="properties">The property collection.</param>
        public NodeHostBuilderContext(IDictionary<object, object> properties)
        {
            Properties = properties ?? throw new System.ArgumentNullException(nameof(properties));
        }

        /// <summary>
        /// A central location for sharing state between components during the host building process.
        /// </summary>
        public IDictionary<object, object> Properties { get; }
    }
}