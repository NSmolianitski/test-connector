namespace TestConnector.HQTestData;

interface ITestConnector
{
    #region Rest

    Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);

    Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from,
        DateTimeOffset? to = null, long? count = 0);

    #endregion

    #region Socket

    event Action<Trade> NewBuyTrade;
    event Action<Trade> NewSellTrade;
    
    // Убрал параметр maxCount, поскольку его нет в API Bitfinex
    void SubscribeTrades(string pair);
    void UnsubscribeTrades(string pair);

    event Action<Candle> CandleSeriesProcessing;

    // Убрал параметры from, to, count, поскольку их нет в API Bitfinex
    void SubscribeCandles(string pair, int periodInSec);

    void UnsubscribeCandles(string pair);

    #endregion
}