using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Events;
using TestConnector.Application.Interfaces;
using TestConnector.Application.Managers;
using TestConnector.Infrastructure.Channels.Candle;
using TestConnector.Infrastructure.Channels.Trade;
using TestConnector.Infrastructure.Processors.Messages;

namespace TestConnector;

public class BitfinexWebsocketClient : IWebSocketClient, IDisposable
{
    private readonly Uri _connectAddress;
    private readonly ClientWebSocket _webSocketClient = new ClientWebSocket();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly ILogger<BitfinexWebsocketClient> _logger;

    private readonly ChannelManager _channelManager;
    private readonly SubscriptionManager _subscriptionManager;
    private readonly MessageProcessor _messageProcessor;
    private readonly EventAggregator _eventAggregator = new();

    public BitfinexWebsocketClient(ILoggerFactory loggerFactory, Uri? uri = null)
    {
        _logger = loggerFactory.CreateLogger<BitfinexWebsocketClient>();
        _connectAddress = uri ?? new Uri("wss://api-pub.bitfinex.com/ws/2");

        _channelManager = new ChannelManager(loggerFactory);
        _subscriptionManager = new SubscriptionManager(loggerFactory.CreateLogger<SubscriptionManager>());
        _messageProcessor = new MessageProcessor(loggerFactory, _subscriptionManager, _channelManager);

        _channelManager.RegisterChannel(new TradeChannelProcessor(loggerFactory.CreateLogger<TradeChannelProcessor>(),
            _eventAggregator));
        _channelManager.RegisterChannel(new CandleChannelProcessor(loggerFactory.CreateLogger<CandleChannelProcessor>(),
            _eventAggregator));
    }

    public async Task ConnectAsync()
    {
        var cancellationToken = _cts.Token;
        await _webSocketClient.ConnectAsync(_connectAddress, cancellationToken);
        _ = ReceiveAsync();
    }

    public async Task CloseAsync()
    {
        await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cts.Token);
    }

    private async Task ReceiveAsync()
    {
        _logger.LogInformation("Receiving messages...");

        var cancellationToken = _cts.Token;
        var buffer = new byte[4096];
        var messageBuffer = new List<byte>();
        while (_webSocketClient.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            }
            else
            {
                messageBuffer.AddRange(buffer.Take(result.Count));

                if (!result.EndOfMessage) 
                    continue;
                
                var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                _logger.LogInformation("Received message: {Message}", message);

                try
                {
                    _messageProcessor.Process(message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error on message processing: {Message}", e.Message);
                }
                messageBuffer.Clear();
            }
        }
    }

    private async Task SendMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await _webSocketClient.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text,
            true, _cts.Token);
    }

    public async Task SubscribeChannel<TRequest>(TRequest request) where TRequest : SubscribeRequest
    {
        if (!_channelManager.TryGetChannel<TRequest>(out var channel))
        {
            _logger.LogWarning("Channel not found for request type: {RequestType}", typeof(TRequest));
            return;
        }

        var subscribeMessage = channel.GetSubscribeMessage(request);
        await SendMessageAsync(subscribeMessage);
    }

    public async Task Unsubscribe<TChannel>(string pair) where TChannel : IChannel
    {
        if (!_subscriptionManager.TryGetSubscription(pair, typeof(TChannel), out var subscription))
        {
            _logger.LogWarning("No subscription found for channel type and pair: {ChannelType}, {Pair}",
                typeof(TChannel), pair);
            return;
        }

        var subscribeMessage = JsonSerializer.Serialize(new
        {
            @event = "unsubscribe",
            chanId = subscription.ChannelId,
        });

        await SendMessageAsync(subscribeMessage);
    }

    public void SubscribeEvent<TEvent>(Action<TEvent> action) where TEvent : IChannelEvent
    {
        _eventAggregator.Subscribe(action);
    }

    public void UnsubscribeEvent<TEvent>(Action<TEvent> action) where TEvent : IChannelEvent
    {
        _eventAggregator.Unsubscribe(action);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _webSocketClient.Dispose();
        _cts.Dispose();
    }
}