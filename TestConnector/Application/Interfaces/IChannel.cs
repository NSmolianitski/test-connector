using System.Text.Json;
using TestConnector.Application.Managers;

namespace TestConnector.Application.Interfaces;

public abstract record SubscribeRequest;

public interface IChannel
{
    public string ChannelName { get; }
    void Handle(Subscription subscription, JsonElement jsonElement);
}

public interface IChannel<in TRequest> : IChannel where TRequest : SubscribeRequest
{
    string GetSubscribeMessage(TRequest request);
}