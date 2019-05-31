using System.Collections.Generic;
using System.Linq;
using DomainBase.Exception;
using DomainBase.Interfaces;
using DomainBase.Repository;
using DomainRepository.EventStore.Event;
using DomainRepository.EventStore.Interfaces;
using EventStore.ClientAPI;

namespace DomainRepository.EventStore
{
    public class EventStoreDomainRepository : DomainRepositoryBase
    {
        private IEventStoreConnection _connection;
        private readonly ITransientDomainEventPublisher _publisher;
        private readonly ICustomiseEventData _customiseEventData;

        private readonly int eventReadLimitCount = 4000;

        public EventStoreDomainRepository(string category,IEventStoreConnection connection, ITransientDomainEventPublisher publisher = null) : this(category, connection, new CustomiseEventData(), publisher)
        { }

        public EventStoreDomainRepository(string category,IEventStoreConnection connection, ICustomiseEventData customiseEventData, ITransientDomainEventPublisher publisher = null) : base(category)
        {
            this._connection = connection;
            this._publisher = publisher;
            this._customiseEventData = customiseEventData;
        }


        public override IEnumerable<IDomainEvent> Save<TAggregate>(TAggregate aggregate, bool isInitial = false)
        {
            try
            {
                var uncommitedEvents = aggregate.UncommitedEvents().ToList();
                var expectedVersion = CalculateExpectedVersion(aggregate, uncommitedEvents);
                if (isInitial)
                    expectedVersion = ExpectedVersion.NoStream;
                var streamName = AggregateToStreamName(aggregate.GetType(), aggregate.AggregateId);
                var eventData = uncommitedEvents.Select(_customiseEventData.Create);
                _connection.AppendToStreamAsync(streamName, expectedVersion, eventData).Wait();
                foreach (var _event in uncommitedEvents)
                    PublishEvent(_event);
                return uncommitedEvents;
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("Unable to save to eventStore", ex);
            }
        }

        public override bool Exists<TResult>(string id)
        {
            try
            {
                var streamName = AggregateToStreamName(typeof(TResult), id);
                var currentSlice = _connection.ReadStreamEventsForwardAsync(streamName, 1, 1, false).Result;
                return currentSlice.Status != SliceReadStatus.StreamNotFound;
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("Unable to retrieve from eventStore", ex);
            }
        }

        public override TResult GetById<TResult>(string id)
        {
            try
            {
                var streamName = AggregateToStreamName(typeof(TResult), id);
                var streamEvents = new List<ResolvedEvent>();
                StreamEventsSlice currentSlice;
                long nextSliceStart = StreamPosition.Start;
                do
                {
                    currentSlice = _connection.ReadStreamEventsForwardAsync(streamName, nextSliceStart, eventReadLimitCount, false).Result;
                    nextSliceStart = currentSlice.NextEventNumber;
                    streamEvents.AddRange(currentSlice.Events);
                } while (!currentSlice.IsEndOfStream && currentSlice.Events.Length > 0);

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                    throw new AggregateNotFoundException("Could not found aggregate of type " + typeof(TResult) + " and id " + id);

                IEnumerable<IDomainEvent> deserializedEvents = currentSlice.Events.Select(e =>
                {
                    var metadata = EventSerialization.DeserializeObject<Dictionary<string, string>>(e.OriginalEvent.Metadata);
                    var eventData = EventSerialization.DeserializeObject(e.OriginalEvent.Data, metadata[_customiseEventData.EventClrTypeHeader]);
                    return eventData as IDomainEvent;
                });
                return BuildAggregate<TResult>(deserializedEvents);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("Unable to retrieve from eventStore", ex);
            }
        }

        public override string GetLast<TResult>()
        {
            try
            {
                var streamNameStartWith = string.Format("{0}-{1}", Category, typeof(TResult).Name);
                AllEventsSlice slice;
                var nextSliceStart = Position.End;
                do
                {
                    slice = _connection.ReadAllEventsBackwardAsync(nextSliceStart, 1, false).Result;
                    foreach (var e in slice.Events)
                    {
                        if (e.Event.EventStreamId.StartsWith(streamNameStartWith))
                            return e.Event.EventStreamId;
                    }
                } while (!slice.IsEndOfStream);

                return null;
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("Unable to retrieve from eventStore", ex);
            }
        }

        protected void PublishEvent(object @event)
        {
            if (_publisher != null)
                _publisher.PublishAsync((dynamic)@event).Wait();
        }


        protected override long CalculateExpectedVersion<T>(IAggregate aggregate, List<T> events)
        {
            var originalVersion = aggregate.Version - events.Count;
            long expectedVersion = originalVersion == -1 ? ExpectedVersion.NoStream : originalVersion;
            return expectedVersion;
        }

    }
}