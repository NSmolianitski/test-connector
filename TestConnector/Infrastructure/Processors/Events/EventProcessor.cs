using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Managers;
using TestConnector.Infrastructure.Processors.Events.Handlers;
using TestConnector.Infrastructure.Processors.Events.Interfaces;
using ErrorEventHandler = TestConnector.Infrastructure.Processors.Events.Handlers.ErrorEventHandler;

namespace TestConnector.Infrastructure.Processors.Events;

public class EventProcessor(
    ILoggerFactory loggerFactory,
    SubscriptionManager subscriptionManager,
    ChannelManager channelManager)
{
    private readonly ILogger<EventProcessor> _logger = loggerFactory.CreateLogger<EventProcessor>();

    private readonly Dictionary<string, IEventHandler> _eventHandlers = new()
    {
        {
            Constants.Events.Subscribed,
            new SubscribedEventHandler(loggerFactory.CreateLogger<SubscribedEventHandler>(), subscriptionManager,
                channelManager)
        },
        {
            Constants.Events.Unsubscribed,
            new UnsubscribedEventHandler(loggerFactory.CreateLogger<UnsubscribedEventHandler>(), subscriptionManager)
        },
        {
            Constants.Events.Error,
            new ErrorEventHandler(loggerFactory.CreateLogger<ErrorEventHandler>())
        },
        {
            Constants.Events.Info,
            new InfoEventHandler(loggerFactory.CreateLogger<InfoEventHandler>())
        }
    };

    public void Process(string eventType, JsonElement eventJson)
    {
        if (!_eventHandlers.TryGetValue(eventType, out var handler))
        {
            _logger.LogWarning("Handler not found for event type: {EventType}", eventType);
            return;
        }

        handler.Handle(eventJson);
    }
}