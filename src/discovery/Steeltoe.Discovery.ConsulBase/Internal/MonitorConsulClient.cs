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
            {
                Config.WaitTime = TimeSpan.FromSeconds(o.WaitTimeoutSeconds);
            }
        }
    }
}