using DomainRepository.EventStore.Configuration;
using EventStore.ClientAPI;

namespace DomainRepository.EventStore.Infrastructure
{
    public class EventStoreConnectionWrapper
    {
        private static IEventStoreConnection _connection;
        protected static EventStoreConfiguration Config { get; set; }

        public static IEventStoreConnection CreateConnection(EventStoreConfiguration config, string connectionName = null, bool keepReconnecting = true)
        {
            Config = config;
            return _connection = _connection ?? Connect(connectionName, keepReconnecting);
        }

        protected static IEventStoreConnection Connect(string connectionName = null, bool keepReconnecting = true)
        {
            return EventStoreConnectionFactory.TCP(Config, connectionName, keepReconnecting);
        }

    }
}
