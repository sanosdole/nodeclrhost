// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace BlazorApp.Hosting
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides Blazor-specific support for <see cref="INodeHost"/>.
    /// </summary>
    public static class NodeHostBuilderExtensions
    {
        private const string BlazorStartupKey = "Blazor.Startup";

        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="INodeHostBuilder" /> to configure.</param>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="INodeHostBuilder"/> for chaining.</returns>
        public static INodeHostBuilder ConfigureServices(this INodeHostBuilder hostBuilder, Action<IServiceCollection> configureDelegate)
        {
            return hostBuilder.ConfigureServices((context, collection) => configureDelegate(collection));
        }

        /// <summary>
        /// Configures the <see cref="INodeHostBuilder"/> to use the provided startup class.
        /// </summary>
        /// <param name="builder">The <see cref="INodeHostBuilder"/>.</param>
        /// <param name="startupType">A type that configures a Blazor application.</param>
        /// <returns>The <see cref="INodeHostBuilder"/>.</returns>
        public static INodeHostBuilder UseBlazorStartup(this INodeHostBuilder builder, Type startupType)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.Properties.ContainsKey(BlazorStartupKey))
            {
                throw new InvalidOperationException("A startup class has already been registered.");
            }

            // It would complicate the implementation to allow multiple startup classes, and we don't
            // really have a need for it.
            builder.Properties.Add(BlazorStartupKey, bool.TrueString);

            // TODO: Use a passed instance here, or even better: Check if this design should be simplified!
            var startup = new ConventionBasedStartup(Activator.CreateInstance(startupType));

            builder.ConfigureServices(startup.ConfigureServices);
            builder.ConfigureServices(s => s.AddSingleton<IBlazorStartup>(startup));

            return builder;
        }

        /// <summary>
        /// Configures the <see cref="INodeHostBuilder"/> to use the provided startup class.
        /// </summary>
        /// <typeparam name="TStartup">A type that configures a Blazor application.</typeparam>
        /// <param name="builder">The <see cref="INodeHostBuilder"/>.</param>
        /// <returns>The <see cref="INodeHostBuilder"/>.</returns>
        public static INodeHostBuilder UseBlazorStartup<TStartup>(this INodeHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return UseBlazorStartup(builder, typeof(TStartup));
        }
    }
}