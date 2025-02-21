using System.Text.Json;

namespace TestConnector.Infrastructure.Channels.Trade;

public static class TradeConverter
{
    public static HQTestData.Trade ToTrade(string pair, List<JsonElement> jsonElement)
    {
        var amount = jsonElement[2].GetDecimal();
        return new HQTestData.Trade
        {
            Pair = pair,
            Id = jsonElement[0].GetInt32().ToString(),
            Time = DateTimeOffset.FromUnixTimeMilliseconds(jsonElement[1].GetInt64()),
            Side = amount >= 0 ? "buy" : "sell",
            Amount = amount,
            Price = jsonElement[3].GetDecimal()
        };
    }
}