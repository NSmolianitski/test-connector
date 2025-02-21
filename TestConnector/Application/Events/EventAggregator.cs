using TestConnector.Application.Interfaces;

namespace TestConnector.Application.Events;

public class EventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = [];

    public void Subscribe<TEvent>(Action<TEvent> action) where TEvent : IChannelEvent
    {
        var eventType = typeof(TEvent);
        if (!_subscribers.TryGetValue(eventType, out var eventSubscribers))
        {
            eventSubscribers = [];
            _subscribers[eventType] = eventSubscribers;
        }

        eventSubscribers.Add(action);
    }
    
    public void Unsubscribe<TEvent>(Action<TEvent> action) where TEvent : IChannelEvent
    {
        var eventType = typeof(TEvent);
        if (_subscribers.TryGetValue(eventType, out var eventSubscribers))
            eventSubscribers.Remove(action);
    }
    
    public void Publish<TEvent>(TEvent @event) where TEvent : IChannelEvent
    {
        var eventType = typeof(TEvent);
        if (!_subscribers.TryGetValue(eventType, out var eventSubscribers))
            return;

        foreach (var subscriber in eventSubscribers)
            ((Action<TEvent>)subscriber)(@event);
    }
}