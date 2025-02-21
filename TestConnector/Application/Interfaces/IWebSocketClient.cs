namespace TestConnector.Application.Interfaces;

public interface IWebSocketClient
{
    Task ConnectAsync();
    Task CloseAsync();
    Task SubscribeChannel<TRequest>(TRequest request) where TRequest : SubscribeRequest;
    Task Unsubscribe<TChannel>(string pair) where TChannel : IChannel;
    void SubscribeEvent<TEvent>(Action<TEvent> handler) where TEvent : IChannelEvent;
    void UnsubscribeEvent<TEvent>(Action<TEvent> handler) where TEvent : IChannelEvent;
}