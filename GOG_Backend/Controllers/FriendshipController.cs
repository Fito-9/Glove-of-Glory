using GOG_Backend.Models.Dto;
using GOG_Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GOG_Backend.Controllers
{
    // Endpoints para todo lo relacionado con amigos.
    [Route("api/[controller]")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly FriendshipService _friendshipService;

        public FriendshipController(FriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        // Envía una solicitud de amistad de un usuario a otro.
        [HttpPost("send-request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            await _friendshipService.SendFriendRequest(request.SenderId, request.ReceiverId);
            return Ok(new { Message = "Solicitud de amistad enviada" });
        }

        // Acepta una solicitud de amistad.
        [HttpPost("accept-request")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] FriendRequestDto request)
        {
            await _friendshipService.AcceptFriendRequest(request.SenderId, request.ReceiverId);
            return Ok(new { Message = "Solicitud de amistad aceptada" });
        }

        // Rechaza una solicitud de amistad.
        [HttpPost("reject-request")]
        public async Task<IActionResult> RejectFriendRequest([FromBody] FriendRequestDto request)
        {
            await _friendshipService.RejectFriendRequest(request.SenderId, request.ReceiverId);
            return Ok(new { Message = "Solicitud de amistad rechazada" });
        }

        // Devuelve la lista de amigos de un usuario.
        [HttpGet("friends/{userId}")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            var friends = await _friendshipService.GetFriends(userId);
            var friendDtos = friends.Select(u => new {
                userId = u.UsuarioId,
                nickname = u.NombreUsuario,
                // Construimos la URL completa del avatar. Si no tiene, usamos el de por defecto.
                ruta = string.IsNullOrEmpty(u.ImagenPerfil)
                       ? $"{Request.Scheme}://{Request.Host}/uploads/default-avatar.png"
                       : $"{Request.Scheme}://{Request.Host}/{u.ImagenPerfil.Replace('\\', '/')}"
            });
            return Ok(friendDtos);
        }

        // Devuelve las solicitudes de amistad pendientes de un usuario.
        [HttpGet("pending-requests/{userId}")]
        public async Task<IActionResult> GetPendingRequests(int userId)
        {
            var pendingFriendships = await _friendshipService.GetPendingRequests(userId);
            var result = pendingFriendships.Select(fr => new FriendRequestDto
            {
                SenderId = fr.SenderId,
                ReceiverId = fr.ReceiverId,
                SenderNickname = fr.Sender?.NombreUsuario ?? "Desconocido",
                SenderAvatar = string.IsNullOrEmpty(fr.Sender?.ImagenPerfil)
                                ? $"{Request.Scheme}://{Request.Host}/uploads/default-avatar.png"
                                : $"{Request.Scheme}://{Request.Host}/{fr.Sender.ImagenPerfil.Replace('\\', '/')}"
            }).ToList();

            return Ok(result);
        }
    }
}