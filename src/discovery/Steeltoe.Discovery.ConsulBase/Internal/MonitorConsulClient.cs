using Consul;
using Microsoft.Extensions.Options;
using System;

namespace Steeltoe.Discovery.Consul.Internal
{
    internal class MonitorConsulClient : ConsulClient
    {
        public MonitorConsulClient(IOptionsMonitor<ConsulClientOptions> optionsMonitor)
        {
            SetConfig(optionsMonitor.CurrentValue);
            optionsMonitor.OnChange(SetConfig);
        }

        private void SetConfig(ConsulClientOptions o)
        {
            Config.Token = o.Token;
            Config.Address = new Uri(o.ServiceUrl);
            Config.Datacenter = o.Datacenter;
            if (o.WaitTimeoutSeconds > 0)
                Config.WaitTime = TimeSpan.FromSeconds(o.WaitTimeoutSeconds);
        }
    }
}