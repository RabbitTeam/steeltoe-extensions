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
        /// Gets or sets a value indicating whether indicates whether the instance should be enabled
        /// for taking traffic as soon as it is registered with consul. Sometimes the application
        /// might need to do some pre-processing before it is ready to take traffic. Configuration
        /// property: consul:instance:instanceEnabledOnInit
        /// </summary>
        public bool IsInstanceEnabledOnInit { get; set; } = true;

        /// <summary>
        /// Gets or sets consul indicates how often (in seconds) the consul client needs to send
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