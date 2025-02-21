using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Interfaces;

namespace TestConnector.Application.Managers;

public class SubscriptionManager(ILogger<SubscriptionManager> logger)
{
    private readonly List<Subscription> _subscriptions = [];

    public bool HasSubscription(Subscription subscription) =>
        _subscriptions.Any(s => s == subscription);

    public bool TryGetSubscription(string pair, Type channelType, [MaybeNullWhen(false)] out Subscription subscription)
    {
        subscription = _subscriptions.FirstOrDefault(s => s.Pair == pair && s.Channel.GetType() == channelType);
        return subscription != null;
    }
    
    public bool TryGetSubscription(int channelId, [MaybeNullWhen(false)] out Subscription subscription)
    {
        subscription = _subscriptions.FirstOrDefault(s => s.ChannelId == channelId);
        return subscription != null;
    }

    public void AddSubscription(Subscription subscription)
    {
        if (HasSubscription(subscription))
        {
            logger.LogWarning("Subscription already exists: {Subscription}", subscription);
            return;
        }

        _subscriptions.Add(subscription);
    }

    public Subscription? RemoveSubscription(string pair, IChannel channel)
    {
        var subscription = _subscriptions
            .FirstOrDefault(s => s.Pair == pair && s.Channel == channel);
        if (subscription == null)
        {
            logger.LogWarning("No subscriptions found for channel and pair: {Channel}, {Pair}", 
                channel, pair);
            return null;
        }

        _subscriptions.Remove(subscription);
        return subscription;
    }

    public Subscription? RemoveSubscription(int channelId)
    {
        var subscription = _subscriptions.FirstOrDefault(s => s.ChannelId == channelId);
        if (subscription == null)
        {
            logger.LogWarning("No subscriptions found with channelId: {ChannelId}", channelId);
            return null;
        }

        _subscriptions.Remove(subscription);
        return subscription;
    }
}