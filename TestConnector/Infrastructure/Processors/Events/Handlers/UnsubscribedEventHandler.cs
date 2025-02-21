using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Managers;
using TestConnector.Constants;
using TestConnector.Infrastructure.Processors.Events.Interfaces;

namespace TestConnector.Infrastructure.Processors.Events.Handlers;

public class UnsubscribedEventHandler(
    ILogger<UnsubscribedEventHandler> logger,
    SubscriptionManager subscriptionManager
) : IEventHandler
{
    public void Handle(JsonElement eventJson)
    {
        logger.LogInformation("Unsubscribe event received: {Event}", eventJson.ToString());
        var channelId = eventJson.GetProperty(ResponseProps.ChanId).GetInt32();

        subscriptionManager.RemoveSubscription(channelId);
    }
}