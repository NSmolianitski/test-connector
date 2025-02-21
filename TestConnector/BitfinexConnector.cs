using Microsoft.Extensions.Logging;
using TestConnector.HQTestData;
using TestConnector.Infrastructure.Channels.Candle;
using TestConnector.Infrastructure.Channels.Candle.Events;
using TestConnector.Infrastructure.Channels.Trade;
using TestConnector.Infrastructure.Channels.Trade.Events;

namespace TestConnector;

public class BitfinexConnector(ILoggerFactory loggerFactory) : ITestConnector
{
    private readonly BitfinexRestClient _restClient = new(new HttpClient());
    private readonly BitfinexWebsocketClient _wsClient = new(loggerFactory);

    #region Rest

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount) =>
        await _restClient.GetNewTradesAsync(pair, maxCount);

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair,
        int periodInSec,
        DateTimeOffset? from,
        DateTimeOffset? to = null,
        long? count = 0
    ) => await _restClient.GetCandleSeriesAsync(pair, periodInSec, from, to, count);

    #endregion

    #region Socket

    public async Task ConnectAsync() => await _wsClient.ConnectAsync();
    public async Task CloseAsync() => await _wsClient.CloseAsync();
    
    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;

    private void OnNewBuyTrade(NewBuyTradeEvent e) => NewBuyTrade?.Invoke(e.Trade);
    private void OnNewSellTrade(NewSellTradeEvent e) => NewSellTrade?.Invoke(e.Trade);

    public void SubscribeTrades(string pair)
    {
        _wsClient.SubscribeChannel(new TradeChannelSubscribeRequest(pair)).Wait();
        _wsClient.SubscribeEvent<NewBuyTradeEvent>(OnNewBuyTrade);
        _wsClient.SubscribeEvent<NewSellTradeEvent>(OnNewSellTrade);
    }

    public void UnsubscribeTrades(string pair)
    {
        _wsClient.Unsubscribe<TradeChannelProcessor>(pair).Wait();
        _wsClient.UnsubscribeEvent<NewBuyTradeEvent>(OnNewBuyTrade);
        _wsClient.UnsubscribeEvent<NewSellTradeEvent>(OnNewSellTrade);
    }

    public event Action<Candle>? CandleSeriesProcessing;

    private void OnCandleSeriesProcessing(CandleSeriesProcessingEvent e) => CandleSeriesProcessing?.Invoke(e.Candle);

    public void SubscribeCandles(string pair, int periodInSec)
    {
        _wsClient.SubscribeChannel(new CandleChannelSubscribeRequest(pair, periodInSec)).Wait();
        _wsClient.SubscribeEvent<CandleSeriesProcessingEvent>(OnCandleSeriesProcessing);
    }

    public void UnsubscribeCandles(string pair)
    {
        _wsClient.Unsubscribe<CandleChannelProcessor>(pair).Wait();
        _wsClient.UnsubscribeEvent<CandleSeriesProcessingEvent>(OnCandleSeriesProcessing);
    }

    #endregion
}