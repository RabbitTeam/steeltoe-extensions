// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Discovery;
using System;
using System.Threading;

namespace Steeltoe.Discovery.Consul
{
    public static class DiscoveryServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulDiscoveryClient(this IServiceCollection services, DiscoveryOptions discoveryOptions, IDiscoveryLifecycle lifecycle = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (discoveryOptions == null)
            {
                throw new ArgumentNullException(nameof(discoveryOptions));
            }

            if (discoveryOptions.ClientType == DiscoveryClientType.EUREKA)
            {
                throw new ArgumentException("Client type EUREKA");
            }

            var clientOptions = discoveryOptions.ClientOptions as ConsulClientOptions;
            services.AddSingleton<IOptionsMonitor<ConsulClientOptions>>(new OptionsMonitorWrapper<ConsulClientOptions>(clientOptions));

            var consulInstanceOptions = discoveryOptions.RegistrationOptions as ConsulInstanceOptions;

            services.AddSingleton<IOptionsMonitor<ConsulInstanceOptions>>(new OptionsMonitorWrapper<ConsulInstanceOptions>(consulInstanceOptions));

            AddConsulServices(services, lifecycle);

            return services;
        }

        public static IServiceCollection AddConsulDiscoveryClient(
            this IServiceCollection services,
            Action<DiscoveryOptions> setupOptions,
            IDiscoveryLifecycle lifecycle = null)
        {
            var discoveryOptions = new DiscoveryOptions();
            setupOptions(discoveryOptions);
            return services.AddConsulDiscoveryClient(discoveryOptions, lifecycle);
        }

        public static IServiceCollection AddConsulDiscoveryClient(
            this IServiceCollection services,
            IConfiguration configuration,
            IDiscoveryLifecycle lifecycle = null)
        {
            var clientConfiguration = configuration.GetSection("consul");

            var clientSection = clientConfiguration.GetSection("client");
            services.Configure<ConsulClientOptions>(clientSection);

            var instanceSection = clientConfiguration.GetSection("instance");
            services.Configure<ConsulInstanceOptions>(instanceSection);
            services.PostConfigure<ConsulInstanceOptions>(options =>
            {
                var applicationSection = configuration.GetSection("spring:application");
                options.Name = applicationSection["name"];
                options.InstanceId = applicationSection["instance_id"];
                if (string.IsNullOrWhiteSpace(options.InstanceId))
                {
                    options.InstanceId = options.GetHostName(false) + ":" + options.Name + ":" + options.Port;
                }
            });

            AddConsulServices(services, lifecycle);

            return services;
        }

        private static void AddConsulServices(IServiceCollection services, IDiscoveryLifecycle lifecycle)
        {
            services.AddSingleton<ConsulDiscoveryClient>();
            if (lifecycle == null)
            {
                services.AddSingleton<IDiscoveryLifecycle, ApplicationLifecycle>();
            }
            else
            {
                services.AddSingleton(lifecycle);
            }

            services.AddSingleton<IDiscoveryClient>(p => p.GetService<ConsulDiscoveryClient>());
        }

        internal class ApplicationLifecycle : IDiscoveryLifecycle
        {
            public ApplicationLifecycle(IApplicationLifetime lifeCycle)
            {
                ApplicationStopping = lifeCycle.ApplicationStopping;
            }

            public CancellationToken ApplicationStopping { get; set; }
        }

        internal class OptionsMonitorWrapper<T> : IOptionsMonitor<T>
        {
            public OptionsMonitorWrapper(T option)
            {
                CurrentValue = option;
            }

            public T CurrentValue { get; }

            public T Get(string name)
            {
                return CurrentValue;
            }

            public IDisposable OnChange(Action<T, string> listener)
            {
                return null;
            }
        }
    }
}