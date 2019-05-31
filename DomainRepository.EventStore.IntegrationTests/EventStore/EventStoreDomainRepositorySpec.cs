using System;
using System.IO;
using DomainBase;
using DomainBase.Interfaces;
using DomainRepository.EventStore.Configuration;
using DomainRepository.EventStore.Infrastructure;
using DomainRepository.EventStore.IntegrationTests.Domain.TestEntityAggregate;
using Machine.Specifications;
using Microsoft.Extensions.Configuration;
using Moq;
using It = Machine.Specifications.It;

namespace DomainRepository.EventStore.IntegrationTests.EventStore
{

    public abstract class EventStoreDomainRepositorySpec
    {
        protected static EventStoreDomainRepository SUT;
        protected static Mock<ITransientDomainEventPublisher> TransientDomainEventPublisherMock;
        protected static System.Exception _exception;
        Establish context = () =>
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
            var eventStoreConfiguration = configuration.GetSection("EventStore").Get<EventStoreConfiguration>();
            var eventStoreConnection = EventStoreConnectionWrapper.CreateConnection(eventStoreConfiguration,null,false);
            TransientDomainEventPublisherMock = new Mock<ITransientDomainEventPublisher>();
            SUT = new EventStoreDomainRepository("test",eventStoreConnection, TransientDomainEventPublisherMock.Object);
            
        };
    }

    [Subject(typeof(EventStoreDomainRepository))]
    public class when_saving_a_new_aggregate: EventStoreDomainRepositorySpec
    {
        protected static TestEntityAggregate TestEntityAggregate;
        protected static string Id;
        Establish context = () =>
        {
            TestEntityAggregate = new TestEntityAggregate();
            Id = string.Format("CreateId-{0}", Guid.NewGuid());
            TestEntityAggregate.Create(new AuditInfo(), Id);
        };


        Because of = () => _exception = Catch.Exception(() => SUT.Save(TestEntityAggregate));

        It should_have_not_raised_any_errors = () => _exception.ShouldBeNull();
        It should_saved_successfully = () => SUT.GetById<TestEntityAggregate>(Id).ShouldNotBeNull();
        It should_saved_the_id_successfully = () => SUT.GetById<TestEntityAggregate>(Id).Id.ShouldNotBeNull();
        It should_publish_the_event= () => TransientDomainEventPublisherMock.Verify(foo=>foo.PublishAsync(Moq.It.IsAny<object>()),Times.AtLeastOnce);
    }


    [Subject(typeof(EventStoreDomainRepository))]
    public class when_saving_10_events_to_the_aggregate : EventStoreDomainRepositorySpec
    {
        protected static TestEntityAggregate TestEntityAggregate;
        protected static string Id;
        protected static int CountLength = 10;

        private Establish context = () =>
        {
            var auditInfo = new AuditInfo();
            TestEntityAggregate = new TestEntityAggregate();
            Id = string.Format("MultipleTest10Id-{0}", Guid.NewGuid());
            TestEntityAggregate.Create(auditInfo, Id);
            for (int i = 1; i <= CountLength; i++)
            {
                TestEntityAggregate.Update(auditInfo, Id, string.Format("Test string value {0}", i));
            }
        };
        Because of = () => _exception = Catch.Exception(() => SUT.Save(TestEntityAggregate));
        It should_have_not_raised_any_errors = () => _exception.ShouldBeNull();
        It should_saved_successfully = () => SUT.GetById<TestEntityAggregate>(Id).ShouldNotBeNull();
        It should_saved_the_id_successfully = () => SUT.GetById<TestEntityAggregate>(Id).Id.ShouldNotBeNull();
        It should_publish_all_the_events = () => TransientDomainEventPublisherMock.Verify(foo => foo.PublishAsync(Moq.It.IsAny<object>()), Times.Exactly(CountLength + 1));
    }


    [Subject(typeof(EventStoreDomainRepository))]
    public class when_saving_over_200_events_to_the_aggregate : EventStoreDomainRepositorySpec
    {
        protected static TestEntityAggregate TestEntityAggregate;
        protected static string Id;
        protected static int CountLength =201;

        private Establish context = () =>
        {
            var auditInfo= new AuditInfo();
            TestEntityAggregate = new TestEntityAggregate();
            Id = string.Format("MultipleTest200PlusId-{0}",Guid.NewGuid());
            TestEntityAggregate.Create(auditInfo, Id);
            for (int i = 1; i <= CountLength; i++)
            {
                TestEntityAggregate.Update(auditInfo,Id,string.Format("Test string value {0}",i));
            }
        };
        Because of = () => _exception = Catch.Exception(() => SUT.Save(TestEntityAggregate));
        It should_have_not_raised_any_errors = () => _exception.ShouldBeNull();
        It should_saved_successfully = () => SUT.GetById<TestEntityAggregate>(Id).ShouldNotBeNull();
        It should_publish_all_the_events = () => TransientDomainEventPublisherMock.Verify(foo => foo.PublishAsync(Moq.It.IsAny<object>()), Times.Exactly(CountLength+1));
    }


    [Subject(typeof(EventStoreDomainRepository))]
    public class when_handling_over_events_limit_of_the_aggregate : EventStoreDomainRepositorySpec
    {
        protected static TestEntityAggregate TestEntityAggregate;
        protected static string Id;
        protected static int CountLength = 5000;

        private Establish context = () =>
        {
            var auditInfo = new AuditInfo();
            TestEntityAggregate = new TestEntityAggregate();
            Id = string.Format("OverEventReadLimitCountId-{0}", Guid.NewGuid());
            TestEntityAggregate.Create(auditInfo, Id);
            for (int i = 1; i <= CountLength; i++)
            {
                TestEntityAggregate.Update(auditInfo, Id, string.Format("Test string value {0}", i));
            }
        };
        Because of = () => _exception = Catch.Exception(() => SUT.Save(TestEntityAggregate));
        It should_have_not_raised_any_errors = () => _exception.ShouldBeNull();
        It should_saved_successfully = () => SUT.GetById<TestEntityAggregate>(Id).ShouldNotBeNull();
        It should_publish_all_the_events = () => TransientDomainEventPublisherMock.Verify(foo => foo.PublishAsync(Moq.It.IsAny<object>()), Times.Exactly(CountLength + 1));
    }


    [Subject(typeof(EventStoreDomainRepository))]
    public class when_getting_last_aggregate_stream : EventStoreDomainRepositorySpec
    {
        protected static TestEntityAggregate TestEntityAggregate;
        protected static string Id;
        protected static int CountLength = 4;
        protected static string Result;
        protected static string LastId;
        private Establish context = () =>
        {
            var auditInfo = new AuditInfo();
            TestEntityAggregate = new TestEntityAggregate();
            for (int i = 1; i <= CountLength; i++)
            {
                LastId = string.Format("LastTest-{0}", Guid.NewGuid());
                TestEntityAggregate.Create(new AuditInfo(), LastId);
                SUT.Save(TestEntityAggregate);
            }
        };
        Because of = () => Result = SUT.GetLast<TestEntityAggregate>();
        It should_have_return_last_stream_id = () => Result.ShouldEqual(string.Format("Test-TestEntityAggregate-{0}", LastId));
    }

 

}
