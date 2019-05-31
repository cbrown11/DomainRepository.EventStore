using System;
using System.Collections.Generic;
using System.Text;
using DomainBase;
using DomainRepository.EventStore.Interfaces;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace DomainRepository.EventStore.Event
{
    public class CustomiseEventData : ICustomiseEventData
    {

        public string EventClrTypeHeader
        {
            get { return "EventClrTypeName"; }
        }

        public CustomiseEventData() {
        }

        public EventData Create(object @event)
            {
                var eventHeaders = new Dictionary<string, string>()
                {
                    { EventClrTypeHeader, @event.GetType().AssemblyQualifiedName},
                    { "Domain", @event.GetType().Assembly.GetName().Name}
                };
                AddAuditInfoToEventHeader(@event, eventHeaders);
                var eventDataHeaders = SerializeObject(eventHeaders);
                var data = SerializeObject(@event);
                var eventData = new EventData(Guid.NewGuid(), @event.GetType().Name, true, data, eventDataHeaders);
                return eventData;
            }

            protected void AddAuditInfoToEventHeader(object @event, Dictionary<string, string> eventHeaders)
            {
                var property = @event.GetType().GetProperty("AuditInfo");
                if (property == null) return;
                var auditInfo = (AuditInfo)property.GetValue(@event);
                eventHeaders.Add("By", auditInfo.By);
                eventHeaders.Add("InitialisedAt", auditInfo.Created.ToString("o"));
            }

            protected byte[] SerializeObject(object obj)
            {
                var jsonObj = JsonConvert.SerializeObject(obj);
                var data = Encoding.UTF8.GetBytes(jsonObj);
                return data;
            }
        }
}
