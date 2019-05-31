

namespace DomainRepository.EventStore.Configuration
{
    public class EventStoreConfiguration
    {
        public string HostName { get; set; }
        public string TCPPort { get; set; }
        public string HTTPPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
