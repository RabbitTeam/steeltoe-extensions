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
using Newtonsoft.Json;
using Steeltoe.Common.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Steeltoe.Discovery.Consul.Internal
{
    internal class ConsulServiceInstance : ConsulServiceInstanceBase, IServiceInstance
    {
        private readonly AgentService _agentService;

        public ConsulServiceInstance(AgentService agentService)
        {
            _agentService = agentService;
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var tags = agentService.Tags;
            if (tags == null || !tags.Any())
            {
                return;
            }

            foreach (var tag in tags)
            {
                const string key = "metadata=";
                if (!tag.StartsWith(key))
                {
                    continue;
                }

                var json = tag.Substring(key.Length);
                Metadata = new Dictionary<string, string>(JsonConvert.DeserializeObject<Dictionary<string, string>>(json), StringComparer.OrdinalIgnoreCase);
            }

            if (Metadata.TryGetValue("IsSecure", out var isSecureString) && bool.TryParse(isSecureString, out var isSecure))
            {
                IsSecure = isSecure;
            }
        }

        #region Overrides of ConsulServiceInstanceBase

        public override string Host => _agentService.Address;

        public override string InstanceId => _agentService.ID;

        #endregion Overrides of ConsulServiceInstanceBase

        #region Implementation of IServiceInstance

        /// <inheritdoc/>
        public string ServiceId => _agentService.Service.ToLower();

        /// <inheritdoc/>
        public int Port => _agentService.Port;

        /// <inheritdoc/>
        public bool IsSecure { get; }

        /// <inheritdoc/>
        public Uri Uri
        {
            get
            {
                var scheme = IsSecure ? "https" : "http";
                return new Uri(scheme + "://" + Host + ":" + Port);
            }
        }

        /// <inheritdoc/>
        public IDictionary<string, string> Metadata { get; }

        #endregion Implementation of IServiceInstance
    }
}