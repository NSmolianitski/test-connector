using TestConnector.Application.Interfaces;

namespace TestConnector.Infrastructure.Channels.Trade.Events;

public record NewSellTradeEvent(HQTestData.Trade Trade) : IChannelEvent;