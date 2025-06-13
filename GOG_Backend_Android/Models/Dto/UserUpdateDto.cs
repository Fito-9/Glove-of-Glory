using System.ComponentModel.DataAnnotations;

namespace GOG_Backend.Models.Dto
{
    public class UserUpdateDto
    {
        public string? NombreUsuario { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public IFormFile? Imagen { get; set; }
    }
}
