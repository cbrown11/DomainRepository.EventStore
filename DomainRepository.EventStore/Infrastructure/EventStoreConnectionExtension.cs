using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;

namespace DomainRepository.EventStore.Infrastructure
{
    public static class EventStoreConnectionExtension
    {

        private const int PageSize = 10;

        public static IEnumerable<T> ReadStreamEventsBackward<T>(this IEventStoreConnection connection, string streamName)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var lastEventNumber = connection.GetLastEventNumber(streamName);
            return lastEventNumber == null
                ? new T[0]
                : ReadResult<T>(connection, streamName, lastEventNumber.Value);
        }

        public static T GetLastEvent<T>(this IEventStoreConnection connection, string streamName)
            where T : class
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var lastEvent = connection.ReadEventAsync(streamName, -1, false).Result;
            if (lastEvent == null || lastEvent.Event == null) return null;
            return lastEvent.Event.Value.ParseJson<T>();
        }

        private static long? GetLastEventNumber(this IEventStoreConnection connection, string streamName)
        {
            var lastEvent = connection.ReadEventAsync(streamName, -1, false).Result;
            if (lastEvent == null || lastEvent.Event == null) return null;
            return lastEvent.Event.Value.OriginalEventNumber;
        }

        private static IEnumerable<T> ReadResult<T>(IEventStoreConnection connection, string streamName, long lastEventNumber)
        {
            var result = new List<T>();
            do
            {
                var events = connection.ReadStreamEventsBackwards(streamName, lastEventNumber);
                result.AddRange(events.Events.Select(e => e.ParseJson<T>()));
                lastEventNumber = events.NextEventNumber;
            } while (lastEventNumber != -1);
            return result;
        }

        private static StreamEventsSlice ReadStreamEventsBackwards(this IEventStoreConnection connection, string streamName, long lastEventNumber)
        {
            return connection.ReadStreamEventsBackwardAsync(streamName,lastEventNumber, PageSize, false).Result;
        }
    }
}
