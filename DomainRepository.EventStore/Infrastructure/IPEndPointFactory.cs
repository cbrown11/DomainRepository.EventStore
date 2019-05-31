using System.Net;

namespace DomainRepository.EventStore.Infrastructure
{
    public class IPEndPointFactory
    {
 
        public static IPEndPoint DefaultTcp()
        {
            return CreateIPEndPoint(1113);
        }

        public static IPEndPoint DefaultHttp()
        {
            return CreateIPEndPoint(2113);
        }

        public static IPEndPoint CreateIPEndPoint(string hostName, string port)
        {
            return new IPEndPoint(EventStoreIP(hostName), EventStorePort(port));
        }

        private static IPEndPoint CreateIPEndPoint(int port)
        {
            var address = IPAddress.Parse("127.0.0.1");
            return new IPEndPoint(address, port);
        }

        private static IPAddress EventStoreIP(string hostname)
        {
                if (string.IsNullOrEmpty(hostname))
                {
                    return IPAddress.Loopback;
                }
                var ipAddresses = Dns.GetHostAddresses(hostname);
                return ipAddresses[0];     
        }

        public static int EventStorePort(string port)
        {

                if (string.IsNullOrEmpty(port))
                {
                    return 1113;
                }
                return int.Parse(port);
            
        }
    }
}
