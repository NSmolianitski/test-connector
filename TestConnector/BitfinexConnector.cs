using TestConnector.Application.Interfaces;
using TestConnector.HQTestData;
using TestConnector.Infrastructure.Channels.Candle;
using TestConnector.Infrastructure.Channels.Candle.Events;
using TestConnector.Infrastructure.Channels.Trade;
using TestConnector.Infrastructure.Channels.Trade.Events;

namespace TestConnector;

public class BitfinexConnector(
    IRestClient restClient,
    IWebSocketClient wsClient
) : ITestConnector
{
    #region Rest

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount) =>
        await restClient.GetNewTradesAsync(pair, maxCount);

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair,
        int periodInSec,
        DateTimeOffset? from,
        DateTimeOffset? to = null,
        long? count = 0
    ) => await restClient.GetCandleSeriesAsync(pair, periodInSec, from, to, count);

    #endregion

    #region Socket
    
    public async Task ConnectAsync() => await wsClient.ConnectAsync();
    public async Task CloseAsync() => await wsClient.CloseAsync();

    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;

    private void OnNewBuyTrade(NewBuyTradeEvent e) => NewBuyTrade?.Invoke(e.Trade);
    private void OnNewSellTrade(NewSellTradeEvent e) => NewSellTrade?.Invoke(e.Trade);

    public void SubscribeTrades(string pair)
    {
        wsClient.SubscribeChannel(new TradeChannelSubscribeRequest(pair)).Wait();
        wsClient.SubscribeEvent<NewBuyTradeEvent>(OnNewBuyTrade);
        wsClient.SubscribeEvent<NewSellTradeEvent>(OnNewSellTrade);
    }

    public void UnsubscribeTrades(string pair)
    {
        wsClient.Unsubscribe<TradeChannelProcessor>(pair).Wait();
        wsClient.UnsubscribeEvent<NewBuyTradeEvent>(OnNewBuyTrade);
        wsClient.UnsubscribeEvent<NewSellTradeEvent>(OnNewSellTrade);
    }

    public event Action<Candle>? CandleSeriesProcessing;

    private void OnCandleSeriesProcessing(CandleSeriesProcessingEvent e) => CandleSeriesProcessing?.Invoke(e.Candle);

    public void SubscribeCandles(string pair, int periodInSec)
    {
        wsClient.SubscribeChannel(new CandleChannelSubscribeRequest(pair, periodInSec)).Wait();
        wsClient.SubscribeEvent<CandleSeriesProcessingEvent>(OnCandleSeriesProcessing);
    }

    public void UnsubscribeCandles(string pair)
    {
        wsClient.Unsubscribe<CandleChannelProcessor>(pair).Wait();
        wsClient.UnsubscribeEvent<CandleSeriesProcessingEvent>(OnCandleSeriesProcessing);
    }

    #endregion
}