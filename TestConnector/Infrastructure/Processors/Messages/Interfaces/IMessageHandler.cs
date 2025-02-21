using System.Text.Json;

namespace TestConnector.Infrastructure.Processors.Messages.Interfaces;

public interface IMessageHandler
{
    void Handle(JsonElement jsonElement);
}