using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using TestConnector.HQTestData;

namespace TestConnector;

public class BitfinexRestClient
{
    private const string BaseAddress = "https://api-pub.bitfinex.com/v2";

    private readonly HttpClient _httpClient;

    public BitfinexRestClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        var path = $"/trades/t{pair}/hist?limit={maxCount}";
        var request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<List<JsonElement>>>(responseString);
        if (result == null)
            throw new InvalidOperationException($"Failed to deserialize response. Response string: {responseString}");

        var trades = result
            .Select(elements =>
            {
                var amount = elements[2].GetDecimal();
                return new Trade
                {
                    Pair = pair,
                    Id = elements[0].GetInt32().ToString(),
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(elements[1].GetInt64()),
                    Side = amount >= 0 ? "buy" : "sell",
                    Amount = Math.Abs(amount),
                    Price = elements[3].GetDecimal()
                };
            });

        return trades;
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair,
        int periodInSec,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        long? count = 0)
    {
        var queryParams = new Dictionary<string, string?>
        {
            {"start", from?.ToUnixTimeMilliseconds().ToString()},
            {"end", to?.ToUnixTimeMilliseconds().ToString()},
            {"limit", count.ToString()},
        };
        var interval = GetClosestInterval(periodInSec);
        var url = QueryHelpers
            .AddQueryString($"{BaseAddress}/candles/trade:{interval}:t{pair}/hist", queryParams);
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<List<JsonElement>>>(responseString);
        if (result == null)
            throw new InvalidOperationException($"Failed to deserialize response. Response string: {responseString}");

        var candles = result.Select(elements =>
        {
            var openPrice = elements[1].GetDecimal();
            var closePrice = elements[2].GetDecimal();
            var highPrice = elements[3].GetDecimal();
            var lowPrice = elements[4].GetDecimal();
            var totalVolume = elements[5].GetDecimal();
            var totalPrice = (openPrice + closePrice + highPrice + lowPrice) / 4m * totalVolume;
            return new Candle
            {
                Pair = pair,
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(elements[0].GetInt64()),
                OpenPrice = openPrice,
                ClosePrice = closePrice,
                HighPrice = highPrice,
                LowPrice = lowPrice,
                TotalVolume = totalVolume,
                TotalPrice = totalPrice
            };
        });

        return candles;
    }

    private string GetClosestInterval(int periodInSec)
    {
        var intervalData = new (int Seconds, string Interval)[]
        {
            (60, "1m"), (300, "5m"), (900, "15m"), (1800, "30m"),
            (3600, "1h"), (10800, "3h"), (21600, "6h"), (43200, "12h"),
            (86400, "1D"), (604800, "1W"), (1209600, "14D"), (2592000, "1M")
        };

        foreach (var (sec, interval) in intervalData)
        {
            if (periodInSec <= sec)
                return interval;
        }
        
        return intervalData[^1].Interval;
    }
}