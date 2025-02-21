using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using TestConnector.HQTestData;
using TestConnector.Infrastructure.Channels.Candle;
using TestConnector.Infrastructure.Channels.Trade;
using TestConnector.Utility;

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
            .Select(elements => TradeConverter.ToTrade(pair, elements));

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
        var interval = TimeUtility.GetClosestInterval(periodInSec);
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

        var candles = result.Select(element => CandleConverter.ToCandle(pair, element));

        return candles;
    }
}