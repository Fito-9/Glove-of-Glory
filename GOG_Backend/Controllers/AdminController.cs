using GOG_Backend.Models.Database;
using GOG_Backend.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace GOG_Backend.Controllers
{
    // Endpoints exclusivos para administradores.
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly MyDbContext _dbContext;

        public AdminController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Método rápido para comprobar si el que hace la petición es admin.
        private async Task<(bool IsAdmin, string ErrorMessage)> IsUserAdmin(string tokenString)
        {
            if (string.IsNullOrEmpty(tokenString))
            {
                return (false, "Token no proporcionado.");
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenString);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return (false, "Token inválido: ID de usuario no encontrado.");
                }

                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "Usuario del token no existe.");
                }

                if (user.Rol != "Admin")
                {
                    return (false, "Acceso denegado. Se requiere rol de administrador.");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error al procesar el token: {ex.Message}");
            }
        }

        // Obtiene la lista de usuarios para el panel de admin.
        [HttpPost("users")]
        public async Task<IActionResult> GetAllUsers([FromBody] AdminRequestDto request)
        {
            var validation = await IsUserAdmin(request.AdminToken);
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

        // Actualiza un usuario.
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto updateUserDto)
        {
            var validation = await IsUserAdmin(updateUserDto.AdminToken);
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

        // Elimina un usuario.
        [HttpPost("users/delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id, [FromBody] AdminRequestDto request)
        {
            var validation = await IsUserAdmin(request.AdminToken);
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