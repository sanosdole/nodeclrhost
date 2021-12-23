// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{            
    using System;

    /// <summary>
    /// Static class that adds extension methods to <see cref="IElectronHostEnvironment"/>.
    /// </summary>
    public static class ElectronHostEnvironmentExtensions
    {
        /// <summary>
        /// Checks if the current hosting environment name is <c>Development</c>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IElectronHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <c>Development</c>, otherwise false.</returns>
        public static bool IsDevelopment(this IElectronHostEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment("Development");
        }

        /// <summary>
        /// Checks if the current hosting environment name is <c>Staging</c>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IElectronHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <c>Staging</c>, otherwise false.</returns>
        public static bool IsStaging(this IElectronHostEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment("Staging");
        }

        /// <summary>
        /// Checks if the current hosting environment name is <c>Production</c>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IElectronHostEnvironment"/>.</param>
        /// <returns>True if the environment name is <c>Production</c>, otherwise false.</returns>
        public static bool IsProduction(this IElectronHostEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment("Production");
        }

        /// <summary>
        /// Compares the current hosting environment name against the specified value.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IElectronHostEnvironment"/>.</param>
        /// <param name="environmentName">Environment name to validate against.</param>
        /// <returns>True if the specified name is the same as the current environment, otherwise false.</returns>
        public static bool IsEnvironment(
            this IElectronHostEnvironment hostingEnvironment,
            string environmentName)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return string.Equals(
                hostingEnvironment.Environment,
                environmentName,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
