using GOG_Backend.Models.Database;
using GOG_Backend.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System; // Necesario para DateTime

namespace GOG_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly TokenValidationParameters _tokenParameters;

        public AdminController(MyDbContext dbContext, TokenValidationParameters tokenParameters)
        {
            _dbContext = dbContext;
            _tokenParameters = tokenParameters;
        }

        // ✅✅✅ INICIO DEL MÉTODO DE VALIDACIÓN MANUAL ALTERNATIVO ✅✅✅
        private async Task<(bool IsAdmin, string ErrorMessage)> ValidateAdminToken()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return (false, "Token de autorización no proporcionado.");
            }

            var tokenString = authHeader.ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(tokenString))
            {
                return (false, "Token inválido.");
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();

                // Paso 1: Leer el token sin validarlo para poder acceder a sus claims.
                // Esto no lanza excepción si ha expirado, lo cual es lo que queremos.
                if (!handler.CanReadToken(tokenString))
                {
                    return (false, "El token no es un JWT válido.");
                }
                var jwtToken = handler.ReadJwtToken(tokenString);

                // Paso 2: Validar la firma manualmente. Si la firma es incorrecta, esto fallará.
                // Usamos los mismos parámetros que en Program.cs para asegurar la consistencia.
                var principal = handler.ValidateToken(tokenString, _tokenParameters, out SecurityToken validatedToken);

                // Paso 3: Validar la expiración manualmente.
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return (false, "El token ha expirado.");
                }

                // Paso 4: Extraer el ID de usuario y comprobar el rol en la base de datos.
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return (false, "Token inválido: no se pudo encontrar el ID de usuario.");
                }

                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "El usuario del token no existe.");
                }

                if (user.Rol != "Admin")
                {
                    return (false, "Acceso denegado. Se requiere rol de administrador.");
                }

                // Si todo ha ido bien, el usuario es un admin válido.
                return (true, null);
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                return (false, "Error de validación: La firma del token es inválida (Key not found).");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return (false, "Error de validación: La firma del token es inválida.");
            }
            catch (Exception ex)
            {
                // Capturamos cualquier otro error inesperado durante la validación.
                return (false, $"Error de validación de token inesperado: {ex.Message}");
            }
        }
        // ✅✅✅ FIN DEL MÉTODO DE VALIDACIÓN MANUAL ALTERNATIVO ✅✅✅

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var validation = await ValidateAdminToken();
            if (!validation.IsAdmin)
            {
                return Unauthorized(new { Message = validation.ErrorMessage });
            }

            var users = await _dbContext.Users
                .Select(u => new AdminUserViewDto
                {
                    UserId = u.UsuarioId,
                    Nickname = u.NombreUsuario,
                    Email = u.Email,
                    PuntuacionElo = u.PuntuacionElo,
                    Rol = u.Rol
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto updateUserDto)
        {
            var validation = await ValidateAdminToken();
            if (!validation.IsAdmin)
            {
                return Unauthorized(new { Message = validation.ErrorMessage });
            }

            var userToUpdate = await _dbContext.Users.FindAsync(id);
            if (userToUpdate == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }

            userToUpdate.NombreUsuario = updateUserDto.Nickname;
            userToUpdate.PuntuacionElo = updateUserDto.PuntuacionElo;
            userToUpdate.Rol = updateUserDto.Rol;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Usuario actualizado correctamente." });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var validation = await ValidateAdminToken();
            if (!validation.IsAdmin)
            {
                return Unauthorized(new { Message = validation.ErrorMessage });
            }

            var userToDelete = await _dbContext.Users.FindAsync(id);
            if (userToDelete == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }

            _dbContext.Users.Remove(userToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Usuario eliminado correctamente." });
        }
    }
}