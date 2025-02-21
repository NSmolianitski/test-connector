using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Events;
using TestConnector.Application.Interfaces;
using TestConnector.Application.Managers;
using TestConnector.Infrastructure.Channels.Trade.Events;

namespace TestConnector.Infrastructure.Channels.Trade;

public class TradeChannelProcessor(
    ILogger<TradeChannelProcessor> logger,
    EventAggregator eventAggregator
) : IChannel<TradeChannelSubscribeRequest>
{
    public string ChannelName => Constants.Channels.Trades;

    public void Handle(Subscription subscription, JsonElement jsonElement)
    {
        var isSnapshot = jsonElement[1].ValueKind == JsonValueKind.Array &&
                         jsonElement[1][0].ValueKind == JsonValueKind.Array;
        if (isSnapshot)
            return;

        var messageType = jsonElement[1].GetString() ?? string.Empty;
        if (messageType != TradeType.TradeExecuted)
        {
            logger.LogWarning("Unexpected message type: {MessageType}", messageType);
            return;
        }
        
        var dataArray = jsonElement[2].EnumerateArray().ToList();
        var trade = TradeConverter.ToTrade(subscription.Pair, dataArray);

        switch (trade.Side)
        {
            case "buy":
                eventAggregator.Publish(new NewBuyTradeEvent(trade));
                break;
            case "sell":
                eventAggregator.Publish(new NewSellTradeEvent(trade));
                break;
        }
    }

    public string GetSubscribeMessage(TradeChannelSubscribeRequest request)
    {
        var subscribeMessage = JsonSerializer.Serialize(new
        {
            @event = "subscribe",
            channel = Constants.Channels.Trades,
            symbol = $"t{request.Pair}"
        });

        return subscribeMessage;
    }
}