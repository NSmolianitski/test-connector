using TestConnector.Application.Interfaces;

namespace TestConnector.Application.Managers;

public record Subscription(IChannel Channel, int ChannelId, string Pair);
