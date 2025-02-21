using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Interfaces;

namespace TestConnector.Application.Managers;

public class ChannelManager(ILoggerFactory loggerFactory)
{
    private readonly ILogger<ChannelManager> _logger = loggerFactory.CreateLogger<ChannelManager>();

    private readonly Dictionary<Type, IChannel> _channels = new();
    private readonly Dictionary<string, IChannel> _channelsByName = new();
    
    public bool TryGetChannel<TRequest>([MaybeNullWhen(false)] out IChannel<TRequest> channel)
        where TRequest : SubscribeRequest
    {
        channel = null;
        if (!_channels.TryGetValue(typeof(TRequest), out var notTypedChannel))
        {
            _logger.LogWarning("Channel not found for request type: {RequestType}", typeof(TRequest));
            return false;
        }

        if (notTypedChannel is not IChannel<TRequest> typedChannel)
        {
            _logger.LogError("Wrong request type for channel: {RequestType}, {Channel}",
                typeof(TRequest), notTypedChannel.GetType());
            return false;
        }

        channel = typedChannel;
        return true;
    }
    
    public bool TryGetChannel(string channelName, [MaybeNullWhen(false)] out IChannel channel)
    {
        channel = null;
        if (!_channelsByName.TryGetValue(channelName, out var foundChannel))
        {
            _logger.LogWarning("Channel with name not found: {ChannelName}", channelName);
            return false;
        }

        channel = foundChannel;
        return true;
    }

    public void RegisterChannel<TRequest>(IChannel<TRequest> channel) where TRequest : SubscribeRequest
    {
        if (!_channels.TryAdd(typeof(TRequest), channel))
        {
            _logger.LogWarning("Channel already exists for channel subscription request type: " +
                               "{ChannelType}, {ChannelSubscriptionType}", channel.GetType(), typeof(TRequest));
        }
        
        _channelsByName.Add(channel.ChannelName, channel);
    }
}