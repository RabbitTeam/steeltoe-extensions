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

using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery.Consul.Discovery;
using Steeltoe.Discovery.Consul.ServiceRegistry;
using Steeltoe.Discovery.Consul.Util;
using System;

namespace Steeltoe.Discovery.Consul.Client
{
    public static class DiscoveryApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDiscoveryClient(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;

            var consulDiscoveryOptions = services.GetService<IOptions<ConsulDiscoveryOptions>>()?.Value;
            var heartbeatOptions = services.GetService<IOptions<HeartbeatOptions>>()?.Value;

            if (consulDiscoveryOptions == null || !consulDiscoveryOptions.Register)
            {
                return app;
            }

            var registration = BuildRegistration(consulDiscoveryOptions, heartbeatOptions);

            var consulRegistration = new ConsulRegistration(registration, consulDiscoveryOptions);

            var consulServiceRegistry = services.GetRequiredService<ConsulServiceRegistry>();
            consulServiceRegistry.RegisterAsync(consulRegistration).GetAwaiter().GetResult();

            if (consulDiscoveryOptions.Deregister)
            {
                var discoveryLifecycle = app.ApplicationServices.GetRequiredService<IDiscoveryLifecycle>();
                discoveryLifecycle.ApplicationStopping.Register(async () =>
                {
                    await consulServiceRegistry.DeregisterAsync(consulRegistration);
                });
            }

            return app;
        }

        public static AgentServiceRegistration BuildRegistration(ConsulDiscoveryOptions options, HeartbeatOptions heartbeatOptions)
        {
            if (!options.Port.HasValue)
            {
                throw new ArgumentException("Port can not be empty.");
            }

            return new AgentServiceRegistration
            {
                Address = options.HostName,
                ID = options.InstanceId,
                Name = options.ServiceName,
                Port = options.Port.Value,
                Tags = options.Tags,
                Check = CreateCheck(options, heartbeatOptions)
            };
        }

        private static AgentServiceCheck CreateCheck(ConsulDiscoveryOptions options, HeartbeatOptions heartbeatOptions)
        {
            if (!options.RegisterHealthCheck && !heartbeatOptions.Enable)
            {
                return null;
            }

            var healthCheckUrl = options.HealthCheckUrl;
            TimeSpan? deregisterCriticalServiceAfter = null;
            TimeSpan? ttl = null;
            TimeSpan? interval = null;
            TimeSpan? timeout = null;

            if (string.IsNullOrWhiteSpace(healthCheckUrl) && !string.IsNullOrWhiteSpace(options.HealthCheckPath))
            {
                var hostString = options.HostName;
                var port = options.Port;
                hostString += ":" + port;

                var healthCheckPath = options.HealthCheckPath;
                if (!healthCheckPath.StartsWith("/"))
                {
                    healthCheckPath = "/" + healthCheckPath;
                }

                healthCheckUrl = $"{options.Scheme}://{hostString}{healthCheckPath}";

                if (!string.IsNullOrWhiteSpace(options.HealthCheckInterval))
                {
                    interval = DateTimeConversions.ToTimeSpan(options.HealthCheckInterval);
                }

                if (!string.IsNullOrWhiteSpace(options.HealthCheckTimeout))
                {
                    timeout = DateTimeConversions.ToTimeSpan(options.HealthCheckTimeout);
                }

                if (!string.IsNullOrWhiteSpace(options.HealthCheckCriticalTimeout))
                {
                    deregisterCriticalServiceAfter = DateTimeConversions.ToTimeSpan(options.HealthCheckCriticalTimeout);
                }
            }

            if (heartbeatOptions.Enable && heartbeatOptions.TtlValue > 0 && !string.IsNullOrWhiteSpace(heartbeatOptions.TtlUnit))
            {
                ttl = DateTimeConversions.ToTimeSpan(heartbeatOptions.TtlValue + heartbeatOptions.TtlUnit);
            }

            var check = new AgentServiceCheck
            {
                HTTP = healthCheckUrl,
                Interval = interval,
                Timeout = timeout,
                DeregisterCriticalServiceAfter = deregisterCriticalServiceAfter,
                TTL = ttl
            };
            return check;
        }
    }
}