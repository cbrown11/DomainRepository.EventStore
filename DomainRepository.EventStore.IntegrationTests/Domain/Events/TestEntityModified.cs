using DomainBase;
using DomainBase.Interfaces;

namespace DomainRepository.EventStore.IntegrationTests.Domain.Events
{

    public class TestEntityModified : DomainEvent, IDomainEvent
    {
        public string TestProperty { get; set; }

        // Need for deserialize 
        public TestEntityModified() { }

        public TestEntityModified(string id, AuditInfo auditInfo, string testProperty) : base(auditInfo, id)
        {
            TestProperty = testProperty;
        }
    }
}
