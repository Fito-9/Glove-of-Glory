using GOG_Backend.Models.Database;
using GOG_Backend.Models.Database.Entities;
using GOG_Backend.Models.Dto;
using GOG_Backend.Recursos;
using GOG_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GOG_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly SimpleSessionService _sessionService;

        public UserController(MyDbContext dbContext, SimpleSessionService sessionService)
        {
            _dbContext = dbContext;
            _sessionService = sessionService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!Request.Headers.TryGetValue("X-Session-Token", out var sessionTokenHeader))
            {
                return Unauthorized("Falta la cabecera de sesión.");
            }

            var sessionToken = sessionTokenHeader.ToString();
            var userId = _sessionService.GetUserIdFromSession(sessionToken);

            if (userId == null)
            {
                return Unauthorized("Sesión inválida o expirada.");
            }

            var user = await _dbContext.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var fullProfile = new UserFullProfileDto
            {
                UserId = user.UsuarioId,
                Nickname = user.NombreUsuario,
                Email = user.Email,
                Elo = user.PuntuacionElo,
                // --- CAMBIO: Usamos directamente la URL guardada en la BD ---
                AvatarUrl = user.ImagenPerfil
            };

            return Ok(fullProfile);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);
            if (user == null) return Unauthorized("Usuario no existe");
            if (!PasswordHelper.Hash(userLoginDto.Password).Equals(user.ContrasenaHash)) return Unauthorized("Contraseña incorrecta");

            var sessionToken = _sessionService.CreateSession(user.UsuarioId);

            return Ok(new
            {
                AccessToken = sessionToken,
                UsuarioId = user.UsuarioId,
                NombreUsuario = user.NombreUsuario,
                // --- CAMBIO: Usamos directamente la URL guardada en la BD ---
                Avatar = user.ImagenPerfil
            });
        }

        [HttpPost("register")]
        // --- CAMBIO: Ya no es [FromForm] porque no subimos archivos, es [FromBody] para recibir JSON ---
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.NombreUsuario == userDto.NombreUsuario))
                return BadRequest("El nombre del usuario ya está en uso");

            // --- CAMBIO: Lógica para obtener la URL del avatar a partir del ID ---
            string? avatarUrl = GetAvatarUrlFromId(userDto.AvatarId);

            var newUser = new User
            {
                NombreUsuario = userDto.NombreUsuario,
                Email = userDto.Email,
                ContrasenaHash = PasswordHelper.Hash(userDto.Password),
                Rol = "Jugador",
                PuntuacionElo = 1200,
                // --- CAMBIO: Guardamos la URL completa del avatar ---
                ImagenPerfil = avatarUrl
            };

            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Usuario registrado con éxito" });
        }

        // --- NUEVO MÉTODO HELPER ---
        private string? GetAvatarUrlFromId(string? avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return null;

            return avatarId.ToLower() switch
            {
                "mario" => "https://www.smashbros.com/assets_v2/img/fighter/mario/main.png",
                "donkey_kong" => "https://www.smashbros.com/assets_v2/img/fighter/donkey_kong/main.png",
                "link" => "https://www.smashbros.com/assets_v2/img/fighter/link/main.png",
                "samus" => "https://www.smashbros.com/assets_v2/img/fighter/samus/main.png",
                _ => null
            };
        }
    }
}