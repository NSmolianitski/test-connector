using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Managers;
using TestConnector.Constants;
using TestConnector.Infrastructure.Processors.Events;
using TestConnector.Infrastructure.Processors.Messages.Interfaces;

namespace TestConnector.Infrastructure.Processors.Messages.Handlers;

public class ObjectMessageHandler(
    ILoggerFactory loggerFactory,
    SubscriptionManager subscriptionManager,
    ChannelManager channelManager
) : IMessageHandler
{
    private readonly ILogger<ObjectMessageHandler> _logger = loggerFactory.CreateLogger<ObjectMessageHandler>();

    private readonly EventProcessor _eventProcessor =
        new EventProcessor(loggerFactory, subscriptionManager, channelManager);

    public void Handle(JsonElement jsonElement)
    {
        if (!jsonElement.TryGetProperty(ResponseProps.Event, out var eventType))
        {
            _logger.LogInformation("Json object is not event: {JsonRoot}", jsonElement.ToString());
            return;
        }

        _eventProcessor.Process(eventType.ToString(), jsonElement);
    }
}