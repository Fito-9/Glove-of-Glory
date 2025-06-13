using GOG_Backend.Models.Dto;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace StrategoBackend.WebSockets
{
    public class WebSocketHandler : IDisposable
    {
        private const int BUFFER_SIZE = 4096;
        private readonly WebSocket _webSocket;
        private readonly byte[] _buffer;

        public int UserId { get; }
        public bool IsOpen => _webSocket.State == WebSocketState.Open;

        public event Func<WebSocketHandler, WebSocketMessageDto, Task> MessageReceived;
        public event Func<WebSocketHandler, Task> Disconnected;

        public WebSocketHandler(int userId, WebSocket webSocket)
        {
            UserId = userId;
            _webSocket = webSocket;
            _buffer = new byte[BUFFER_SIZE];
        }

        public async Task HandleAsync()
        {
            await SendAsync(new WebSocketMessageDto { Type = "welcome", Payload = $"Bienvenido, tu id es {UserId}" });

            while (IsOpen)
            {
                string message = await ReadAsync();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    WebSocketMessageDto messageDto;
                    try
                    {
                        // Intentamos entender el mensaje como un objeto JSON.
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        messageDto = JsonSerializer.Deserialize<WebSocketMessageDto>(message, options);
                        if (messageDto == null || string.IsNullOrEmpty(messageDto.Type))
                        {
                            messageDto = new WebSocketMessageDto { Type = "text", Payload = message };
                        }
                    }
                    catch
                    {
                        // Si no es JSON, lo tratamos como texto plano.
                        messageDto = new WebSocketMessageDto { Type = "text", Payload = message };
                    }

                    if (MessageReceived != null)
                    {
                        await MessageReceived.Invoke(this, messageDto);
                    }
                }
            }

            // Si salimos del bucle, es que se desconectó. 
            if (Disconnected != null)
            {
                await Disconnected.Invoke(this);
            }
        }

        // Lee un mensaje completo del socket.
        private async Task<string> ReadAsync()
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    ms.Write(_buffer, 0, result.Count);
                }
                else if (result.CloseStatus.HasValue)
                {
                    await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            } while (!result.EndOfMessage);

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        // Envía un mensaje de texto al cliente.
        public async Task SendAsync(string message)
        {
            if (IsOpen)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        // Envía un objeto como JSON al cliente.
        public async Task SendAsync(WebSocketMessageDto dto)
        {
            string json = JsonSerializer.Serialize(dto);
            await SendAsync(json);
        }

        // Limpia la conexión.
        public void Dispose()
        {
            _webSocket.Dispose();
        }
    }
}