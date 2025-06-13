using GOG_Backend.Models.Database;
using GOG_Backend.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GOG_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly MyDbContext _dbContext;

        public ProfileController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Función privada para obtener el ID de usuario desde el token
        private int? GetUserIdFromToken(string tokenString)
        {
            if (string.IsNullOrEmpty(tokenString)) return null;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenString);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        [HttpPost] // Usamos POST para poder enviar el token en el body
        public async Task<IActionResult> GetUserProfile([FromBody] AdminRequestDto request)
        {
            int? userId = GetUserIdFromToken(request.AdminToken);
            if (userId == null)
            {
                return Unauthorized(new { Message = "Token inválido o no proporcionado." });
            }

            var user = await _dbContext.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }

            var userProfile = new UserProfileDto
            {
                Nickname = user.NombreUsuario,
                Email = user.Email,
                PuntuacionElo = user.PuntuacionElo,
                AvatarUrl = string.IsNullOrEmpty(user.ImagenPerfil)
                    ? $"{Request.Scheme}://{Request.Host}/uploads/default-avatar.png"
                    : $"{Request.Scheme}://{Request.Host}/{user.ImagenPerfil.Replace('\\', '/')}"
            };

            return Ok(userProfile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateProfileDto updateDto)
        {
            int? userId = GetUserIdFromToken(updateDto.AdminToken);
            if (userId == null)
            {
                return Unauthorized(new { Message = "Token inválido o no proporcionado." });
            }

            var user = await _dbContext.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }

            // Actualizar nickname si se proporcionó
            if (!string.IsNullOrWhiteSpace(updateDto.Nickname))
            {
                user.NombreUsuario = updateDto.Nickname;
            }

            // Actualizar imagen de perfil si se subió una nueva
            var imageFile = Request.Form.Files.FirstOrDefault();
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                string uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                string newAvatarPath = Path.Combine("uploads", uniqueFileName);
                string fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                user.ImagenPerfil = newAvatarPath;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Perfil actualizado correctamente." });
        }
    }
}