using System.Globalization;
using FluentAssertions;
using Moq;
using TestConnector.Application.Interfaces;

namespace TestConnector.Tests;

public class BitfinexRestTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly BitfinexConnector _connector;

    public BitfinexRestTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        _httpClient = new HttpClient();
        var restClient = new BitfinexRestClient(_httpClient);
        var wsClient = new Mock<IWebSocketClient>();
        _connector = new BitfinexConnector(restClient, wsClient.Object);
    }

    [Fact]
    public async Task GetNewTradesAsync_ShouldReturn_Trades()
    {
        // Arrange
        const string pair = "BTCUSD";
        const int maxCount = 5;

        // Act
        var trades = await _connector.GetNewTradesAsync(pair, maxCount);
        var tradesList = trades.ToList();

        // Assert
        tradesList.Should().NotBeNull();
        tradesList.Should().HaveCountLessThanOrEqualTo(maxCount);
    }

    [Fact]
    public async Task GetCandleSeriesAsync_ShouldReturn_Candles()
    {
        // Arrange
        const string pair = "BTCUSD";
        const int period = 60;
        var from = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        var candles = await _connector.GetCandleSeriesAsync(pair, period, from);
        var candlesList = candles.ToList();

        // Assert
        candlesList.Should().NotBeNull();
        candlesList.Should().NotBeEmpty();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}