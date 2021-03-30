// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Modified by Daniel Martin for nodeclrhost

namespace ElectronHostedBlazor.Hosting
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    // Keeping this simple for now to focus on predictable and reasonable behaviors.
    // Startup in WebHost supports lots of things we don't yet support, and some we
    // may never support.
    //
    // Possible additions:
    // - environments
    // - case-insensitivity (makes sense with environments)
    //
    // Likely never:
    // - statics
    // - DI into constructor
    internal class ConventionBasedStartup : IBlazorStartup
    {
        public ConventionBasedStartup(object instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public object Instance { get; }

        public void Configure(IComponentsApplicationBuilder app, IServiceProvider services)
        {
            try
            {
                var method = GetConfigureMethod();
                Debug.Assert(method != null);

                var parameters = method.GetParameters();
                var arguments = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    arguments[i] = parameter.ParameterType == typeof(IComponentsApplicationBuilder)
                                       ? app
                                       : services.GetRequiredService(parameter.ParameterType);
                }

                method.Invoke(Instance, arguments);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        internal MethodInfo GetConfigureMethod()
        {
            var methods = Instance.GetType()
                                  .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                  .Where(m => string.Equals(m.Name, "Configure", StringComparison.Ordinal))
                                  .ToArray();

            if (methods.Length == 1)
            {
                return methods[0];
            }
            else if (methods.Length == 0)
            {
                throw new InvalidOperationException("The startup class must define a 'Configure' method.");
            }
            else
            {
                throw new InvalidOperationException("Overloading the 'Configure' method is not supported.");
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                var method = GetConfigureServicesMethod();
                if (method != null)
                {
                    method.Invoke(Instance, new object[] { services });
                }
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        public void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            try
            {
                var method = GetConfigureLoggingMethod();
                if (method != null)
                {
                    method.Invoke(Instance, new object[] { loggingBuilder });
                }
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

        internal MethodInfo GetConfigureServicesMethod()
        {
            return Instance.GetType()
                           .GetMethod(
                               "ConfigureServices",
                               BindingFlags.Public | BindingFlags.Instance,
                               null,
                               new Type[] { typeof(IServiceCollection), },
                               Array.Empty<ParameterModifier>());
        }

        internal MethodInfo GetConfigureLoggingMethod()
        {
            return Instance.GetType()
                           .GetMethod(
                               "ConfigureLogging",
                               BindingFlags.Public | BindingFlags.Instance,
                               null,
                               new Type[] { typeof(ILoggingBuilder), },
                               Array.Empty<ParameterModifier>());
        }
    }
}
