using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Managers;
using TestConnector.Constants;
using TestConnector.Infrastructure.Processors.Events.Interfaces;

namespace TestConnector.Infrastructure.Processors.Events.Handlers;

public class SubscribedEventHandler(
    ILogger logger,
    SubscriptionManager subscriptionManager,
    ChannelManager channelManager
) : IEventHandler
{
    public void Handle(JsonElement eventJson)
    {
        logger.LogInformation("Subscribe event received: {Event}", eventJson.ToString());

        var channelName = eventJson.GetProperty(ResponseProps.Channel).GetString() ?? string.Empty;

        if (!channelManager.TryGetChannel(channelName, out var channel))
        {
            logger.LogWarning("Channel with name not found: {Name}", channelName);
            return;
        }

        string? pair = null;
        if (eventJson.TryGetProperty(ResponseProps.Pair, out var pairElement))
        {
            pair = pairElement.GetString();
        }
        else if (eventJson.TryGetProperty(ResponseProps.Key, out pairElement))
        {
            pair = pairElement.GetString()?.Split(':').Last().Substring(1);
        }

        if (pair == null)
        {
            logger.LogError("Pair not found in event: {Event}", eventJson.ToString());
            return;
        }

        var newSubscription = new Subscription(
            channel,
            eventJson.GetProperty(ResponseProps.ChanId).GetInt32(),
            pair);

        if (subscriptionManager.HasSubscription(newSubscription))
        {
            logger.LogWarning("Subscription already exists: {Subscription}", newSubscription);
            return;
        }

        subscriptionManager.AddSubscription(newSubscription);
    }
}