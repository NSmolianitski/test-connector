using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Managers;
using TestConnector.Infrastructure.Processors.Messages.Handlers;
using TestConnector.Infrastructure.Processors.Messages.Interfaces;

namespace TestConnector.Infrastructure.Processors.Messages;

public class MessageProcessor(
    ILoggerFactory loggerFactory,
    SubscriptionManager subscriptionManager,
    ChannelManager channelManager)
{
    private readonly ILogger<MessageProcessor> _logger = loggerFactory.CreateLogger<MessageProcessor>();

    private readonly Dictionary<JsonValueKind, IMessageHandler> _messageHandlers = new()
    {
        {JsonValueKind.Object, new ObjectMessageHandler(loggerFactory, subscriptionManager, channelManager)},
        {JsonValueKind.Array, new ArrayMessageHandler(loggerFactory, subscriptionManager)},
    };

    public void Process(string message)
    {
        var jsonDocument = JsonDocument.Parse(message);
        var jsonRoot = jsonDocument.RootElement;

        if (!_messageHandlers.TryGetValue(jsonRoot.ValueKind, out var handler))
        {
            _logger.LogWarning("Unknown message type: {MessageType}", jsonRoot.ValueKind.ToString());
            return;
        }

        handler.Handle(jsonRoot);
    }
}