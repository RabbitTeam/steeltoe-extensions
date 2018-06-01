using Microsoft.Extensions.Logging;
using Steeltoe.Common.Discovery;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Time_UI.Services
{
    public class TimeService : ITimeService
    {
        private readonly DiscoveryHttpClientHandler _handler;
        private const string CURRENT_TIME_URL = "http://timeService/api/currentTime";

        public TimeService(IDiscoveryClient discoveryClient, ILogger<DiscoveryHttpClientHandler> discoveryHandlerLogger)
        {
            _handler = new DiscoveryHttpClientHandler(discoveryClient, discoveryHandlerLogger);
        }

        #region Implementation of ITimeService

        public async Task<DateTime> GetNowAsync()
        {
            var client = GetClient();
            var result = await client.GetStringAsync(CURRENT_TIME_URL);
            return DateTime.Parse(result);
        }

        #endregion Implementation of ITimeService

        private HttpClient GetClient()
        {
            var client = new HttpClient(_handler, false);
            return client;
        }
    }
}