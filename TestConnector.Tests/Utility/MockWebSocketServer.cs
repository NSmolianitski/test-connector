using Fleck;

namespace TestConnector.Tests.Utility;

public class MockWebSocketServer(string uri)
{
    private readonly WebSocketServer _server = new(uri);
    private IWebSocketConnection? _socket;

    public void Start()
    {
        _server.Start(socket =>
        {
            _socket = socket;

            socket.OnOpen = () => Console.WriteLine("[MockWebSocketServer] Client connected.");
            socket.OnClose = () => Console.WriteLine("[MockWebSocketServer] Client disconnected.");
            socket.OnMessage = message => Console.WriteLine($"[MockWebSocketServer] Received: {message}");
        });
    }

    public void SendMessage(string message)
    {
        _socket?.Send(message);
    }

    public void Stop()
    {
        _server.Dispose();
    }
}