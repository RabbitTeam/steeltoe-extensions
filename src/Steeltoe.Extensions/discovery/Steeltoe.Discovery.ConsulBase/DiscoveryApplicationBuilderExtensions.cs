using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common.Discovery;

namespace Steeltoe.Discovery.Consul
{
    public static class DiscoveryApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDiscoveryClient(this IApplicationBuilder app)
        {
            var service = app.ApplicationServices.GetRequiredService<IDiscoveryClient>();
            return app;
        }
    }
}