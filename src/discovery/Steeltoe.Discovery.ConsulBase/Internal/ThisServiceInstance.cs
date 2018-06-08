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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Steeltoe.Discovery.Consul.Internal
{
    internal class ThisServiceInstance : IServiceInstance
    {
        public string InstanceId => InstanceOptions.InstanceId;

        private readonly IOptionsMonitor<ConsulInstanceOptions> _optionsMonitor;

        private ConsulInstanceOptions InstanceOptions => _optionsMonitor.CurrentValue;

        public ThisServiceInstance(IOptionsMonitor<ConsulInstanceOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        #region Implementation of IServiceInstance

        /// <inheritdoc/>
        public string ServiceId => InstanceOptions.Name;

        private string _host;

        /// <inheritdoc/>
        public string Host
        {
            get
            {
                if (_host != null)
                {
                    return _host;
                }

                return _host = InstanceOptions.GetHostName(false);
            }
            set => _host = value;
        }

        /// <inheritdoc/>
        public int Port => InstanceOptions.Port;

        /// <inheritdoc/>
        public bool IsSecure
        {
            get => InstanceOptions.SecurePortEnabled;
            set => InstanceOptions.SecurePortEnabled = value;
        }

        /// <inheritdoc/>
        public Uri Uri
        {
            get
            {
                var scheme = IsSecure ? "https" : "http";
                var uri = new Uri(scheme + "://" + Host.ToLower() + ":" + Port);
                return uri;
            }
        }

        /// <inheritdoc/>
        public IDictionary<string, string> Metadata
        {
            get => InstanceOptions.Metadata;
            set => InstanceOptions.Metadata = value;
        }

        #endregion Implementation of IServiceInstance

        internal AgentServiceRegistration BuildRegistration(ConsulInstanceOptions instanceOptions)
        {
            var registration = new AgentServiceRegistration
            {
                Address = Host.ToLower(),
                Check = new AgentServiceCheck
                {
                    TTL = TimeSpan.FromSeconds(instanceOptions.LeaseExpirationDurationInSeconds),
                    Status = instanceOptions.IsInstanceEnabledOnInit ? HealthStatus.Passing : HealthStatus.Maintenance
                },
                ID = InstanceId,
                Name = ServiceId,
                Port = Port
            };

            if (Metadata != null && Metadata.Any())
            {
                registration.Tags = new[]
                {
                    "metadata=" + JsonConvert.SerializeObject(
                        Metadata,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                };
            }

            return registration;
        }
    }
}