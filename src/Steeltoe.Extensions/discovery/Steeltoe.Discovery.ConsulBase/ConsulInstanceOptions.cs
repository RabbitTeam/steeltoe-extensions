using Steeltoe.Common.Discovery;
using System;
using System.Collections.Generic;

namespace Steeltoe.Discovery.Consul
{
    public class ConsulInstanceOptions : ConsulServiceInstanceBase, IDiscoveryRegistrationOptions
    {
        public ConsulInstanceOptions()
        {
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// consul Gets or sets indicates how often (in seconds) the consul client needs to send
        /// heartbeats to consul server to indicate that it is still alive. If the heartbeats are not
        /// received for the period specified in <see cref="LeaseExpirationDurationInSeconds"/>,
        /// consul server will removethe instance from its view, there by disallowing traffic to this
        /// instance. Note that the instance could still not take traffic if it implements
        /// HealthCheckCallback and then decides to make itself unavailable. Configuration property: consul:instance:leaseRenewalIntervalInSeconds
        /// </summary>
        public int LeaseRenewalIntervalInSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets indicates the time in seconds that the consul server waits since it received
        /// the last heartbeat before it can remove this instance from its view and there by
        /// disallowing traffic to this instance.
        ///
        /// Setting this value too long could mean that the traffic could be routed to the instance
        /// even though the instance is not alive. Setting this value too small could mean, the
        /// instance may be taken out of traffic because of temporary network glitches.This value to
        /// be set to atleast higher than the value specified in <see
        /// cref="LeaseRenewalIntervalInSeconds"/> Configuration property: consul:instance:leaseExpirationDurationInSeconds
        /// </summary>
        public int LeaseExpirationDurationInSeconds { get; set; } = 90;

        public bool SecurePortEnabled
        {
            get => Metadata.TryGetValue("IsSecure", out var isSecure) && isSecure == bool.TrueString;
            set => Metadata["IsSecure"] = value ? bool.TrueString : bool.FalseString;
        }

        public string Name { get; set; }
        public int Port { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}