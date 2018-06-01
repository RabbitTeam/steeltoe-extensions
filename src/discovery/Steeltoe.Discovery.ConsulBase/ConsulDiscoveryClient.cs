using Consul;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery.Consul.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steeltoe.Discovery.Consul
{
    public class ConsulDiscoveryClient : IDiscoveryClient, IDisposable
    {
        private readonly IOptionsMonitor<ConsulClientOptions> _optionsMonitor;
        private readonly IOptionsMonitor<ConsulInstanceOptions> _instanceOptionsMonitor;
        private volatile IList<ConsulServiceInstance> _instances;
        private int _shutdown;
        private TimedTask _cacheRefreshTimer;
        private TimedTask _heartBeatTimer;
        private readonly ThisServiceInstance _thisServiceInstance;

        public IConsulClient ConsulClient { get; private set; }
        private ConsulClientOptions ClientOptions => _optionsMonitor.CurrentValue;
        private ConsulInstanceOptions InstanceOptions => _instanceOptionsMonitor.CurrentValue;

        public ConsulDiscoveryClient(IOptionsMonitor<ConsulClientOptions> optionsMonitor,
            IOptionsMonitor<ConsulInstanceOptions> instanceOptionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
            _instanceOptionsMonitor = instanceOptionsMonitor;
            ConsulClient = new MonitorConsulClient(optionsMonitor);

            var options = ClientOptions;
            var instanceOptions = InstanceOptions;

            if (ClientOptions.ShouldFetchRegistry)
                _cacheRefreshTimer = new TimedTask("Query", CacheRefreshTaskAsync, options.RegistryFetchIntervalSeconds * 1000);

            if (ClientOptions.ShouldRegisterWithConsul)
                _heartBeatTimer = new TimedTask("HeartBeat", HeartBeatTaskAsync, instanceOptions.LeaseRenewalIntervalInSeconds * 1000);

            _thisServiceInstance = new ThisServiceInstance(instanceOptionsMonitor);

            if (options.ShouldRegisterWithConsul || options.ShouldFetchRegistry)
                Task.Run(async () =>
                    {
                        if (options.ShouldRegisterWithConsul)
                            await RegisterAsync();
                        if (options.ShouldFetchRegistry)
                            _instances = await LoadConsulInstancesAsync();
                    })
                    .GetAwaiter()
                    .GetResult();
        }

        #region Implementation of IDiscoveryClient

        /// <inheritdoc/>
        public IServiceInstance GetLocalServiceInstance()
        {
            return _thisServiceInstance;
        }

        /// <inheritdoc/>
        public IList<IServiceInstance> GetInstances(string serviceId)
        {
            if (serviceId == null)
                throw new ArgumentNullException(nameof(serviceId));

            return _instances.OfType<IServiceInstance>().Where(i => serviceId.Equals(i.ServiceId, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        /// <inheritdoc/>
        public async Task ShutdownAsync()
        {
            var shutdown = Interlocked.Exchange(ref _shutdown, 1);
            if (shutdown > 0)
                return;

            _cacheRefreshTimer?.Dispose();
            _cacheRefreshTimer = null;
            _heartBeatTimer?.Dispose();
            _heartBeatTimer = null;

            if (ClientOptions.ShouldRegisterWithConsul)
                await UnregisterAsync();

            ConsulClient?.Dispose();
            ConsulClient = null;
        }

        /// <inheritdoc/>
        public string Description { get; } = "HashiCorp Consul Client";

        /// <inheritdoc/>
        public IList<string> Services => GetServices();

        #endregion Implementation of IDiscoveryClient

        #region Private Method

        private async void HeartBeatTaskAsync()
        {
            if (_shutdown > 0)
                return;

            await PassTtlAsync("heartBeat");
        }

        private async void CacheRefreshTaskAsync()
        {
            if (_shutdown > 0)
                return;

            _instances = await LoadConsulInstancesAsync();
        }

        private async Task<bool> RegisterAsync()
        {
            var agentService = ConsulClient.Agent;
            var result = await agentService.ServiceRegister(_thisServiceInstance.BuildRegistration(InstanceOptions));
            return result.StatusCode == HttpStatusCode.OK;
        }

        private async Task<bool> UnregisterAsync()
        {
            var agentService = ConsulClient.Agent;
            var result = await agentService.ServiceDeregister(_thisServiceInstance.InstanceId);
            return result.StatusCode == HttpStatusCode.OK;
        }

        private async Task PassTtlAsync(string note)
        {
            if (!ClientOptions.ShouldRegisterWithConsul)
                return;
            await TryTtlAsync(() => ConsulClient.Agent.PassTTL(GetCheckId(_thisServiceInstance.InstanceId), note));
        }

        private async Task TryTtlAsync(Func<Task> ttlTask)
        {
            try
            {
                await ttlTask();
            }
            catch (ConsulRequestException cre) when (cre.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (!cre.Message.Contains("does not have associated TTL"))
                    return;
                await RegisterAsync();
                await ttlTask();
            }
        }

        private static string GetCheckId(string instanceId)
        {
            return "service:" + instanceId;
        }

        private async Task<IList<ConsulServiceInstance>> LoadConsulInstancesAsync()
        {
            var result = await ConsulClient.Health.State(HealthStatus.Passing);
            var passServiceIds = result.Response.Select(i => i.ServiceID).Where(i => !string.IsNullOrEmpty(i)).ToArray();

            var agentServices = (await ConsulClient.Agent.Services()).Response.Values;
            return agentServices
                .Where(i => !string.IsNullOrEmpty(i.ID))
                .Where(i => passServiceIds.Contains(i.ID))
                .Select(s => new ConsulServiceInstance(s)).ToArray();
        }

        private IList<string> GetServices()
        {
            return !_instances.Any() ? Enumerable.Empty<string>().ToArray() : _instances.Select(i => i.ServiceId?.ToLowerInvariant()).Where(i => i != null).ToArray();
        }

        #endregion Private Method

        #region IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            ShutdownAsync().GetAwaiter().GetResult();
        }

        #endregion IDisposable
    }
}