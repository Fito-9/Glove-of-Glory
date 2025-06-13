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

            string avatarUrl = string.IsNullOrEmpty(user.ImagenPerfil)
                ? null
                : $"{Request.Scheme}://{Request.Host}/{user.ImagenPerfil.Replace("\\", "/")}";

            var fullProfile = new UserFullProfileDto
            {
                UserId = user.UsuarioId,
                Nickname = user.NombreUsuario,
                Email = user.Email,
                Elo = user.PuntuacionElo,
                AvatarUrl = avatarUrl
            };

            return Ok(fullProfile);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto userLoginDto)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == userLoginDto.Email);
            if (user == null) return Unauthorized("Usuario no existe");
            if (!PasswordHelper.Hash(userLoginDto.Password).Equals(user.ContrasenaHash)) return Unauthorized("Contraseña incorrecta");

            var sessionToken = _sessionService.CreateSession(user.UsuarioId);

            string avatarUrl = string.IsNullOrEmpty(user.ImagenPerfil) ? null : $"{Request.Scheme}://{Request.Host}/{user.ImagenPerfil.Replace("\\", "/")}";

            return Ok(new
            {
                AccessToken = sessionToken,
                UsuarioId = user.UsuarioId,
                NombreUsuario = user.NombreUsuario,
                Avatar = avatarUrl
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegisterDto userDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.NombreUsuario == userDto.NombreUsuario))
                return BadRequest("El nombre del usuario ya está en uso");

            string? avatarPath = null;
            if (userDto.Imagen != null && userDto.Imagen.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(userDto.Imagen.FileName)}";
                avatarPath = Path.Combine("uploads", uniqueFileName);
                string fullPath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await userDto.Imagen.CopyToAsync(fileStream);
                }
            }

            var newUser = new User
            {
                NombreUsuario = userDto.NombreUsuario,
                Email = userDto.Email,
                ContrasenaHash = PasswordHelper.Hash(userDto.Password),
                Rol = "Jugador",
                PuntuacionElo = 1200,
                ImagenPerfil = avatarPath
            };

            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Usuario registrado con éxito" });
        }
    }
}