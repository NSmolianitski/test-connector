using TestConnector.Application.Interfaces;

namespace TestConnector.Infrastructure.Channels.Trade.Events;

public record NewBuyTradeEvent(HQTestData.Trade Trade) : IChannelEvent;