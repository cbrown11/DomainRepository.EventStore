using System;
using System.Collections.Generic;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace DomainRepository.EventStore.Event
{
    public class EventSerialization
    {

        public static string EventClrTypeHeader = "EventClrTypeName";

        public static T DeserializeObject<T>(byte[] data)
        {
            return (T)(DeserializeObject(data, typeof(T).AssemblyQualifiedName));
        }

        public static object DeserializeObject(byte[] data, string typeName)
        {
            var jsonString = Encoding.UTF8.GetString(data);
            var type = Type.GetType(typeName);
            return JsonConvert.DeserializeObject(jsonString, type);
        }

        public static object DeserializeEvent(RecordedEvent originalEvent)
        {
            if (originalEvent.Metadata != null)
            {
                var metadata = DeserializeObject<Dictionary<string, string>>(originalEvent.Metadata);
                if (metadata != null && metadata.ContainsKey(EventClrTypeHeader))
                {
                    var eventData = DeserializeObject(originalEvent.Data, metadata[EventClrTypeHeader]);
                    return eventData;
                }
            }
            return null;
        }
    }
}
