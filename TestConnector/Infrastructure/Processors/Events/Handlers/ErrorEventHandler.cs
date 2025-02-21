using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Infrastructure.Processors.Events.Interfaces;

namespace TestConnector.Infrastructure.Processors.Events.Handlers;

public class ErrorEventHandler(ILogger<ErrorEventHandler> logger) : IEventHandler
{
    public void Handle(JsonElement eventJson)
    {
        var errorCode = eventJson.TryGetProperty("code", out var code) ? code.GetInt32() : 0;
        var message = eventJson.TryGetProperty("msg", out var msg) ? msg.GetString() : null;
        logger.LogError("Error received: {ErrorCode}, {ErrorMessage}", errorCode, message);
    }
}