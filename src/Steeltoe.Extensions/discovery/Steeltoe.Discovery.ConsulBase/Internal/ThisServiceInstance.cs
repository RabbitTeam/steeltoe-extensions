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
        private readonly IOptionsMonitor<ConsulInstanceOptions> _optionsMonitor;

        public ThisServiceInstance(IOptionsMonitor<ConsulInstanceOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        private ConsulInstanceOptions InstanceOptions => _optionsMonitor.CurrentValue;
        public string InstanceId => InstanceOptions.InstanceId;

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
                    return _host;
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
                var uri = new Uri(scheme + "://" + Host + ":" + Port);
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
                Address = Host,
                Check = new AgentServiceCheck
                {
                    TTL = TimeSpan.FromSeconds(instanceOptions.LeaseExpirationDurationInSeconds),
                    Status = HealthStatus.Passing
                },
                ID = InstanceId,
                Name = ServiceId,
                Port = Port
            };

            if (Metadata != null && Metadata.Any())
            {
                registration.Tags = new[]
                {
                    "metadata=" + JsonConvert.SerializeObject(Metadata,
                        new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                };
            }

            return registration;
        }
    }
}