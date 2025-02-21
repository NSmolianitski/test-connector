using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Application.Events;
using TestConnector.Application.Interfaces;
using TestConnector.Application.Managers;
using TestConnector.Infrastructure.Channels.Candle.Events;
using TestConnector.Utility;

namespace TestConnector.Infrastructure.Channels.Candle;

public class CandleChannelProcessor(
    ILogger<CandleChannelProcessor> logger,
    EventAggregator eventAggregator
) : IChannel<CandleChannelSubscribeRequest>
{
    public string ChannelName => Constants.Channels.Candles;

    public void Handle(Subscription subscription, JsonElement jsonElement)
    {
        var isSnapshot = jsonElement[1].ValueKind == JsonValueKind.Array &&
                         jsonElement[1][0].ValueKind == JsonValueKind.Array;
        if (isSnapshot)
            return;
        
        var dataArray = jsonElement[1].EnumerateArray().ToList();
        var candle = CandleConverter.ToCandle(subscription.Pair, dataArray);
        eventAggregator.Publish(new CandleSeriesProcessingEvent(candle));
    }

    public string GetSubscribeMessage(CandleChannelSubscribeRequest request)
    {
        var interval = TimeUtility.GetClosestInterval(request.PeriodInSec);
        var subscribeMessage = JsonSerializer.Serialize(new
        {
            @event = "subscribe",
            channel = Constants.Channels.Candles,
            key = $"trade:{interval}:t{request.Pair}"
        });

        return subscribeMessage;
    }
}