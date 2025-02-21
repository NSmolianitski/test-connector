using TestConnector.Application.Interfaces;

namespace TestConnector.Infrastructure.Channels.Candle.Events;

public record CandleSeriesProcessingEvent(HQTestData.Candle Candle) : IChannelEvent;