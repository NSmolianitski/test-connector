using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestConnector.Infrastructure.Processors.Events.Interfaces;

namespace TestConnector.Infrastructure.Processors.Events.Handlers;

public class InfoEventHandler(ILogger<InfoEventHandler> logger) : IEventHandler
{
    public void Handle(JsonElement eventJson)
    {
        logger.LogInformation("Info event received: {Event}", eventJson.ToString());
    }
}