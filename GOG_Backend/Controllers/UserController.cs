using GOG_Backend.Models.Database;
using GOG_Backend.Models.Database.Entities;
using GOG_Backend.Models.Dto;
using GOG_Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GOG_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly TokenValidationParameters _tokenParameters;

        public UserController(MyDbContext dbContext, TokenValidationParameters tokenParameters)
        {
            _dbContext = dbContext;
            _tokenParameters = tokenParameters;
        }

        [HttpGet]
        public IEnumerable<UserSummaryDto> GetUsers()
        {
            return _dbContext.Users.Select(u => new UserSummaryDto
            {
                UserId = u.UsuarioId,
                Nickname = u.NombreUsuario,
                Ruta = string.IsNullOrEmpty(u.ImagenPerfil)
                        ? $"{Request.Scheme}://{Request.Host}/uploads/default-avatar.png"
                        : $"{Request.Scheme}://{Request.Host}/{u.ImagenPerfil.Replace('\\', '/')}",
                PuntuacionElo = u.PuntuacionElo
            }).ToList();
        }

        [HttpGet("ranking")]
        public IEnumerable<UserSummaryDto> GetRanking()
        {
            return _dbContext.Users
                .OrderByDescending(u => u.PuntuacionElo)
                .Select(u => new UserSummaryDto
                {
                    UserId = u.UsuarioId,
                    Nickname = u.NombreUsuario,
                    Ruta = string.IsNullOrEmpty(u.ImagenPerfil)
                            ? $"{Request.Scheme}://{Request.Host}/uploads/default-avatar.png"
                            : $"{Request.Scheme}://{Request.Host}/{u.ImagenPerfil.Replace('\\', '/')}",
                    PuntuacionElo = u.PuntuacionElo
                }).ToList();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegisterDto userDto)
        {
            if (_dbContext.Users.Any(u => u.NombreUsuario == userDto.NombreUsuario))
                return BadRequest("El nombre del usuario ya está en uso");

            string? avatarPath = null;
            if (userDto.Imagen != null && userDto.Imagen.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

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

            return Ok(new { Message = "Usuario registrado con éxito", Avatar = avatarPath });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto userLoginDto)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == userLoginDto.Email);
            if (user == null)
                return Unauthorized("Usuario no existe");

            if (!PasswordHelper.Hash(userLoginDto.Password).Equals(user.ContrasenaHash))
                return Unauthorized("Contraseña incorrecta");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()),
                    new Claim(ClaimTypes.Name, user.NombreUsuario)
                }),
                Expires = DateTime.UtcNow.AddDays(5),
                SigningCredentials = new SigningCredentials(
                    _tokenParameters.IssuerSigningKey,
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            string avatarUrl = string.IsNullOrEmpty(user.ImagenPerfil)
                ? $"{Request.Scheme}://{Request.Host}/uploads/default-avatar.png"
                : $"{Request.Scheme}://{Request.Host}/{user.ImagenPerfil.Replace('\\', '/')}";

            return Ok(new
            {
                AccessToken = accessToken,
                UsuarioId = user.UsuarioId,
                NombreUsuario = user.NombreUsuario,
                PuntuacionElo = user.PuntuacionElo,
                Avatar = avatarUrl
            });
        }
    }
}