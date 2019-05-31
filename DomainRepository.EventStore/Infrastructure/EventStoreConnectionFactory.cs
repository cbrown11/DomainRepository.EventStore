using DomainRepository.EventStore.Configuration;
using EventStore.ClientAPI;

namespace DomainRepository.EventStore.Infrastructure
{
    public static class EventStoreConnectionFactory
    {
        public static IEventStoreConnection DefaultTCP()
        {
            var settings = ConnectionSettings.Create()
                .KeepReconnecting()
                .UseConsoleLogger();

            var connection = EventStoreConnection.Create(settings, IPEndPointFactory.DefaultTcp());
            connection.ConnectAsync();
            return connection;
        }

        public static IEventStoreConnection HTTP(EventStoreConfiguration config, string connectionName = null,bool keepReconnecting = true)
        {
            return Connect(config, config.HTTPPort, connectionName, keepReconnecting);
        }

        public static IEventStoreConnection TCP(EventStoreConfiguration config, string connectionName = null, bool keepReconnecting = true)
        {
            return Connect(config, config.TCPPort, connectionName, keepReconnecting);
        }


        public static IEventStoreConnection Connect(EventStoreConfiguration config, string port, string connectionName = null, bool keepReconnecting =true)
        {
            var settings = ConnectionSettings.Create();
            if(keepReconnecting)
                settings.KeepReconnecting();
            return Connect(settings, config,port, connectionName);
        }

        public static IEventStoreConnection Connect(ConnectionSettingsBuilder connectionSettingsBuilder, EventStoreConfiguration config, string port, string connectionName = null)
        {
            connectionSettingsBuilder
                .UseConsoleLogger()
                .SetDefaultUserCredentials(EventStoreCredentials.Config(config));
            var endPoint = IPEndPointFactory.CreateIPEndPoint(config.HostName, port);
            var connection = EventStoreConnection.Create(connectionSettingsBuilder, endPoint, connectionName);
            connection.ConnectAsync();
            return connection;
        }

    }
}
