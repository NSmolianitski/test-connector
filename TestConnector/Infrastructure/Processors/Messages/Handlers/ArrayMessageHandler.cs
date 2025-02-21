using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Managers;
using TestConnector.Infrastructure.Processors.Messages.Interfaces;

namespace TestConnector.Infrastructure.Processors.Messages.Handlers;

public class ArrayMessageHandler(
    ILoggerFactory loggerFactory,
    SubscriptionManager subscriptionManager
) : IMessageHandler
{
    private readonly ILogger<ArrayMessageHandler> _logger = loggerFactory.CreateLogger<ArrayMessageHandler>();

    public void Handle(JsonElement jsonElement)
    {
        var channelId = jsonElement[0].GetInt32();
        if (!subscriptionManager.TryGetSubscription(channelId, out var subscription))
        {
            _logger.LogWarning("Subscription not found for channel id: {ChannelId}", channelId);
            return;
        }

        var isHeartbeat = jsonElement[1].ValueKind == JsonValueKind.String && jsonElement[1].GetString() == "hb";
        if (isHeartbeat)
            return;

        var channel = subscription.Channel;
        channel.Handle(subscription, jsonElement);
    }
}