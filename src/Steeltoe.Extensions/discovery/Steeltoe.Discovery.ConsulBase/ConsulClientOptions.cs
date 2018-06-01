using Steeltoe.Common.Discovery;

namespace Steeltoe.Discovery.Consul
{
    public class ConsulClientOptions : IDiscoveryClientOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether indicates whether or not this instance should
        /// register its information with consul server for discovery by others.
        ///
        /// In some cases, you do not want your instances to be discovered whereas you just want do
        /// discover other instances.
        ///
        /// Configuration property: consul:client:shouldRegisterWithConsul
        /// </summary>
        public bool ShouldRegisterWithConsul { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether this client should fetch consul
        /// registry information from consul server. Configuration property: consul:client:shouldFetchRegistry
        /// </summary>
        public bool ShouldFetchRegistry { get; set; } = true;

        /// <summary>
        /// Gets or sets indicates how often(in seconds) to fetch the registry information from the
        /// consul server. Configuration property: consul:client:registryFetchIntervalSeconds
        /// </summary>
        public int RegistryFetchIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets comma delimited list of URls to use in contacting the Consul Server
        /// Configuration property: consul:client:serviceUrl
        /// </summary>
        public string ServiceUrl { get; set; }

        public string Datacenter { get; set; }
        public string Token { get; set; }
        public int WaitTimeoutSeconds { get; set; }
    }
}