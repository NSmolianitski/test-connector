using System.Text.Json;

namespace TestConnector.Infrastructure.Channels.Candle;

public static class CandleConverter
{
    public static HQTestData.Candle ToCandle(string pair, List<JsonElement> jsonElement)
    {
        var openPrice = jsonElement[1].GetDecimal();
        var closePrice = jsonElement[2].GetDecimal();
        var highPrice = jsonElement[3].GetDecimal();
        var lowPrice = jsonElement[4].GetDecimal();
        var totalVolume = jsonElement[5].GetDecimal();
        var totalPrice = (openPrice + closePrice + highPrice + lowPrice) / 4m * totalVolume;
            
        return new HQTestData.Candle
        {
            Pair = pair,
            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(jsonElement[0].GetInt64()),
            OpenPrice = openPrice,
            ClosePrice = closePrice,
            HighPrice = highPrice,
            LowPrice = lowPrice,
            TotalVolume = totalVolume,
            TotalPrice = totalPrice
        };
    }
}