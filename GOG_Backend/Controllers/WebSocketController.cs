using GOG_Backend.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Linq;

namespace GOG_Backend.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly WebSocketNetwork _webSocketNetwork;

        public WebSocketController(WebSocketNetwork webSocketNetwork)
        {
            _webSocketNetwork = webSocketNetwork;
        }

        // El frontend llama a esta ruta para establecer la conexión WebSocket.
        [HttpGet("connect")]
        public async Task<IActionResult> ConnectAsync()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return BadRequest("Esto no es una petición WebSocket.");

            // El token viene en la URL 
            string token = HttpContext.Request.Query["token"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Falta el token.");

            // Sacamos el ID de usuario del token para saber quién se conecta.
            int? userId = GetUserIdFromToken(token);
            if (!userId.HasValue)
                return Unauthorized("Token inválido.");

            // Si todo está bien, aceptamos la conexión y se la pasamos a nuestro gestor principal.
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _webSocketNetwork.HandleAsync(webSocket, userId.Value);

            return Ok(); // La respuesta HTTP es solo para confirmar que la conexión se estableció.
        }

        // Un endpoint simple para saber quién está conectado ahora mismo.
        [HttpGet("online-users")]
        public IActionResult GetOnlineUsers()
        {
            return Ok(_webSocketNetwork.GetConnectedUsers());
        }

        // Función para leer el ID de usuario de dentro del token.
        private int? GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                    return userId;
            }
            catch { } // Si el token es basura, simplemente devolvemos null.
            return null;
        }
    }
}