using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TestConnector.Application.Interfaces;
using TestConnector.HQTestData;
using TestConnector.Tests.Utility;

namespace TestConnector.Tests;

public sealed class BitfinexWebSocketTests
{
    private readonly MockWebSocketServer _mockServer;
    private readonly BitfinexConnector _connector;

    public BitfinexWebSocketTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        
        const string mockServerAddress = "ws://127.0.0.1:8181";
        _mockServer = new MockWebSocketServer(mockServerAddress);
        _mockServer.Start();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var restClient = new Mock<IRestClient>();
        var wsClient = new BitfinexWebsocketClient(loggerFactory, new Uri(mockServerAddress));
        _connector = new BitfinexConnector(restClient.Object, wsClient);
        
        _connector.ConnectAsync().Wait();
    }
    
    [Fact]
    public async Task BitfinexConnector_ShouldTriggerBuyTradeEvent_WhenBuyTradeMessageIsReceived()
    {
        // Arrange
        Trade? receivedTrade = null;
        _connector.NewBuyTrade += trade => receivedTrade = trade;

        const string pair = "BTCUSD";
        
        _connector.SubscribeTrades(pair);
        await Task.Delay(100);
        
        const string fakeSubscribeMessage =
            """{"event":"subscribed","channel":"trades","chanId":17470,"symbol":"tBTCUSD","pair":"BTCUSD"}""";
        _mockServer.SendMessage(fakeSubscribeMessage);
        await Task.Delay(100);
        
        // Act
        const string fakeEventMessage = """[17470,"te",[401597395,1574694478808,0.005,7245.3]]""";
        _mockServer.SendMessage(fakeEventMessage);
        await Task.Delay(100);

        // Assert
        receivedTrade.Should().NotBeNull();
        receivedTrade.Id.Should().Be("401597395");
        receivedTrade.Time.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1574694478808));
        receivedTrade.Price.Should().Be(7245.3m);
        receivedTrade.Amount.Should().Be(0.005m);
        receivedTrade.Side.Should().Be("buy");
        receivedTrade.Pair.Should().Be(pair);

        // Cleanup
        await _connector.CloseAsync();
        _mockServer.Stop();
    }

    [Fact]
    public async Task BitfinexConnector_ShouldTriggerSellTradeEvent_WhenSellTradeMessageIsReceived()
    {
        // Arrange
        Trade? receivedTrade = null;
        _connector.NewSellTrade += trade => receivedTrade = trade;

        const string pair = "BTCEUR";

        _connector.SubscribeTrades(pair);
        await Task.Delay(100);

        const string fakeSubscribeMessage =
            """{"event":"subscribed","channel":"trades","chanId":123,"symbol":"tBTCEUR","pair":"BTCEUR"}""";
        _mockServer.SendMessage(fakeSubscribeMessage);
        await Task.Delay(100);

        // Act
        const string fakeEventMessage = """[123,"te",[789,1574694478999,-0.25,12.78]]""";
        _mockServer.SendMessage(fakeEventMessage);
        await Task.Delay(100);

        // Assert
        receivedTrade.Should().NotBeNull();
        receivedTrade.Id.Should().Be("789");
        receivedTrade.Time.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1574694478999));
        receivedTrade.Price.Should().Be(12.78m);
        receivedTrade.Amount.Should().Be(-0.25m);
        receivedTrade.Side.Should().Be("sell");
        receivedTrade.Pair.Should().Be(pair);
        
        // Cleanup
        await _connector.CloseAsync();
        _mockServer.Stop();
    }
    
    [Fact]
    public async Task BitfinexConnector_ShouldTriggerCandleEvent_WhenCandleMessageIsReceived()
    {
        // Arrange
        Candle? receivedCandle = null;
        _connector.CandleSeriesProcessing += candle => receivedCandle = candle;

        const string pair = "BTCUSD";
        const decimal openPrice = 7399.9m;
        const decimal closePrice = 7379.7m;
        const decimal highPrice = 7399.9m;
        const decimal lowPrice = 7371.8m;
        const decimal volume = 41.63633658m;
        const long openTime = 1574698200000;
        const int periodInSec = 60;

        _connector.SubscribeCandles(pair, periodInSec);
        await Task.Delay(100);

        const string fakeSubscribeMessage =
            """{"event":"subscribed","channel":"candles","chanId":343351,"key":"trade:1m:tBTCUSD"}""";
        _mockServer.SendMessage(fakeSubscribeMessage);
        await Task.Delay(100);

        // Act
        var fakeEventMessage = $"[343351,[{openTime},{openPrice},{closePrice},{highPrice},{lowPrice},{volume}]]";
        _mockServer.SendMessage(fakeEventMessage);
        await Task.Delay(100);

        // Assert
        const decimal expectedTotalPrice = (highPrice + lowPrice + closePrice + openPrice) / 4m * volume;
        receivedCandle.Should().NotBeNull();
        receivedCandle.Pair.Should().Be(pair);
        receivedCandle.OpenTime.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(openTime));
        receivedCandle.OpenPrice.Should().Be(openPrice);
        receivedCandle.ClosePrice.Should().Be(closePrice);
        receivedCandle.HighPrice.Should().Be(highPrice);
        receivedCandle.LowPrice.Should().Be(lowPrice);
        receivedCandle.TotalPrice.Should().Be(expectedTotalPrice);
        receivedCandle.TotalVolume.Should().Be(volume);
        
        // Cleanup
        await _connector.CloseAsync();
        _mockServer.Stop();
    }
}