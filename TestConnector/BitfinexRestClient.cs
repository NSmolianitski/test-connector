using System.Net.Http.Headers;
using System.Text.Json;
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
}