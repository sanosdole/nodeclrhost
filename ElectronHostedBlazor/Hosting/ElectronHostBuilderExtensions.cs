// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides Blazor-specific support for <see cref="IElectronHost"/>.
    /// </summary>
    public static class ElectronHostBuilderExtensions
    {
        private const string BlazorStartupKey = "Blazor.Startup";

        /// <summary>
        /// Adds services to the container. This can be called multiple times and the results will be additive.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IElectronHostBuilder" /> to configure.</param>
        /// <param name="configureDelegate"></param>
        /// <returns>The same instance of the <see cref="IElectronHostBuilder"/> for chaining.</returns>
        public static IElectronHostBuilder ConfigureServices(this IElectronHostBuilder hostBuilder, Action<IServiceCollection> configureDelegate)
        {
            return hostBuilder.ConfigureServices((context, collection) => configureDelegate(collection));
        }

        /// <summary>
        /// Configures the <see cref="IElectronHostBuilder"/> to use the provided startup class.
        /// </summary>
        /// <param name="builder">The <see cref="IElectronHostBuilder"/>.</param>
        /// <param name="startupType">A type that configures a Blazor application.</param>
        /// <returns>The <see cref="IElectronHostBuilder"/>.</returns>
        public static IElectronHostBuilder UseBlazorStartup(this IElectronHostBuilder builder, Type startupType)
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
        /// Configures the <see cref="IElectronHostBuilder"/> to use the provided startup class.
        /// </summary>
        /// <typeparam name="TStartup">A type that configures a Blazor application.</typeparam>
        /// <param name="builder">The <see cref="IElectronHostBuilder"/>.</param>
        /// <returns>The <see cref="IElectronHostBuilder"/>.</returns>
        public static IElectronHostBuilder UseBlazorStartup<TStartup>(this IElectronHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return UseBlazorStartup(builder, typeof(TStartup));
        }
    }
}