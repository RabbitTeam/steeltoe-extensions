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
                return;

            foreach (var tag in tags)
            {
                const string key = "metadata=";
                if (!tag.StartsWith(key))
                    continue;
                var json = tag.Substring(key.Length);
                Metadata = new Dictionary<string, string>(JsonConvert.DeserializeObject<Dictionary<string, string>>(json), StringComparer.OrdinalIgnoreCase);
            }

            if (Metadata.TryGetValue("IsSecure", out var isSecureString) &&
                bool.TryParse(isSecureString, out var isSecure))
                IsSecure = isSecure;
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