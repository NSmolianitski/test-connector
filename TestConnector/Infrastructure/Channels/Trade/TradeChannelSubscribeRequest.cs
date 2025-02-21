using TestConnector.Application.Interfaces;

namespace TestConnector.Infrastructure.Channels.Trade;

public record TradeChannelSubscribeRequest(string Pair) : SubscribeRequest;