using System.Text.Json;

namespace TestConnector.Infrastructure.Processors.Events.Interfaces;

public interface IEventHandler
{
    void Handle(JsonElement eventJson);
}