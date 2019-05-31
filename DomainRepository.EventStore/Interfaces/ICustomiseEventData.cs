using EventStore.ClientAPI;

namespace DomainRepository.EventStore.Interfaces
{
    public interface ICustomiseEventData
    {
        string EventClrTypeHeader { get; }
        EventData Create(object @event);
    }
}