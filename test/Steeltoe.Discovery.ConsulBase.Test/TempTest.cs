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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery.Consul;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Steeltoe.Discovery.ConsulBase.Test
{
    public class TempTest
    {
        [Fact]
        public async Task Test()
        {
            // leaseRenewalIntervalInSeconds leaseExpirationDurationInSeconds
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(
                        "spring:application:name",
                        "ConsoleApp"),
                    new KeyValuePair<string, string>(
                        "spring:application:instance_id",
                        "ConsoleApp1"),
                    new KeyValuePair<string, string>(
                        "consul:client:serviceUrl",
                        "http://localhost:8500"),
                    new KeyValuePair<string, string>(
                        "consul:client:registryFetchIntervalSeconds",
                        "2"),
                    new KeyValuePair<string, string>(
                        "consul:instance:port",
                        "5000"),
                    new KeyValuePair<string, string>(
                        "consul:instance:host",
                        "192.168.1.100"),
                    new KeyValuePair<string, string>(
                        "consul:instance:leaseRenewalIntervalInSeconds",
                        "6"),
                    new KeyValuePair<string, string>(
                        "consul:instance:leaseExpirationDurationInSeconds",
                        "4")
                })
                .Build();
            var services = new ServiceCollection()
                .AddOptions()
                .AddConsulDiscoveryClient(configuration)
                .BuildServiceProvider();

            var discoveryClient = services.GetService<IDiscoveryClient>();

            var localServiceInstance = discoveryClient.GetLocalServiceInstance();
            foreach (var service in discoveryClient.Services)
            {
                var instances = discoveryClient.GetInstances(service);
                Assert.NotEmpty(instances);
                instances = discoveryClient.GetInstances(service.ToUpper());
                Assert.NotEmpty(instances);
            }

            Assert.Equal("192.168.1.100", localServiceInstance.Host);
            Assert.Equal(5000, localServiceInstance.Port);
            Assert.False(localServiceInstance.IsSecure);
            Assert.Equal("ConsoleApp", localServiceInstance.ServiceId);
            Assert.Equal(new Uri("http://192.168.1.100:5000"), localServiceInstance.Uri);

            await Task.Delay(TimeSpan.FromSeconds(5));

            Assert.Empty(discoveryClient.GetInstances(localServiceInstance.ServiceId));

            await discoveryClient.ShutdownAsync();

            await Task.Delay(TimeSpan.FromSeconds(3));

            Assert.False(discoveryClient.Services.Contains(localServiceInstance.ServiceId));
        }
    }
}