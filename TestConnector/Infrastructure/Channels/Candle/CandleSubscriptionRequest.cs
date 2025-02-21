using TestConnector.Application.Interfaces;

namespace TestConnector.Infrastructure.Channels.Candle;

public record CandleChannelSubscribeRequest(string Pair, int PeriodInSec) : SubscribeRequest;