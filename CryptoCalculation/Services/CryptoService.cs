using System.Net.Http.Headers;
using System.Text.Json;

namespace CryptoCalculation.Services;

public class CryptoService(HttpClient httpClient)
{
    private const string ExchangeUrl = "https://api-pub.bitfinex.com/v2/calc/fx";

    private record UsdCoef(string Currency, decimal Rate);

    private async Task<UsdCoef> GetExchangeRatesAsync(string ccy1, string ccy2)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(ExchangeUrl),
            Headers = {{"accept", "application/json"},},
            Content = new StringContent($"{{\"ccy1\":\"{ccy1}\",\"ccy2\":\"{ccy2}\"}}")
            {
                Headers = {ContentType = new MediaTypeHeaderValue("application/json")}
            }
        };
        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<JsonElement>>(responseString);
        if (result == null)
            throw new InvalidOperationException($"Failed to deserialize response. Response string: {responseString}");

        var rate = result[0].GetDecimal();
        return new UsdCoef(ccy1, rate);
    }

    public async Task<Dictionary<string, decimal>> GetCryptoPricesAsync(Dictionary<string, decimal> portfolioBalance)
    {
        const string usd = "USD";
        List<Task<UsdCoef>> requestTasks = [];
        requestTasks.AddRange(portfolioBalance.Keys.Select(currency => GetExchangeRatesAsync(currency, usd)));

        var results = await Task.WhenAll(requestTasks);
        return results.ToDictionary(c => c.Currency, c => c.Rate);
    }

    public async Task<Dictionary<string, decimal>> CalculateTotalBalance(Dictionary<string, decimal> portfolioBalance)
    {
        var pricesInUsd = await GetCryptoPricesAsync(portfolioBalance);

        var currencyBalanceInUsd = new Dictionary<string, decimal>();
        foreach (var (currencyName, usdRate) in pricesInUsd)
        {
            var balanceInUsd = portfolioBalance[currencyName] * usdRate;
            currencyBalanceInUsd.Add(currencyName, balanceInUsd);
        }
        
        var totalBalance = new Dictionary<string, decimal>();
        var sumInUsd = currencyBalanceInUsd.Values.Sum();
        totalBalance.Add("USDT", sumInUsd);
        foreach (var (currencyName, usdRate) in pricesInUsd)
        {
            var totalBalanceInCurrency = sumInUsd / usdRate;
            totalBalance.Add(currencyName, totalBalanceInCurrency);
        }

        return totalBalance;
    }
}