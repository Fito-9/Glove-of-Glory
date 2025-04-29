using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GOG_Backend.Models.Dto
{
    public class UserRegisterDto
    {
        [Required]
        public string NombreUsuario { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}
