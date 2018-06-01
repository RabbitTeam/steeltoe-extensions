using System;
using System.Net;

namespace Steeltoe.Discovery.Consul
{
    public class ConsulServiceInstanceBase
    {
        public virtual string InstanceId { get; set; }
        private string _host;

        public virtual string Host
        {
            get => _host ?? (_host = GetHostName(true));
            set => _host = value ?? string.Empty;
        }

        public virtual string GetHostName(bool refresh)
        {
            if (refresh || string.IsNullOrEmpty(Host))
            {
                Host = ResolveHostName();
            }

            return Host;
        }

        protected virtual string ResolveHostName()
        {
            string result = null;
            try
            {
                result = Dns.GetHostName();
                if (!string.IsNullOrEmpty(result))
                {
                    var response = Dns.GetHostEntry(result);
                    return response.HostName;
                }
            }
            catch (Exception)
            {
                // Ignore
            }

            return result;
        }
    }
}