using DomainRepository.EventStore.Configuration;
using EventStore.ClientAPI.SystemData;

namespace DomainRepository.EventStore.Infrastructure
{
    public class EventStoreCredentials
    {
        private static readonly UserCredentials Credentials = new UserCredentials("admin", "changeit");

        public static UserCredentials Default { get { return Credentials; } }

        public static UserCredentials Config(EventStoreConfiguration config)
        {
            if (string.IsNullOrEmpty(config.Username))
                return Credentials;
            else
                return new UserCredentials(config.Username, config.Password);
        }
    }
}
