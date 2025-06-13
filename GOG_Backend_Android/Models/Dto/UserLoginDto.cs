using System.ComponentModel.DataAnnotations;

namespace GOG_Backend.Models.Dto
{
    public class UserLoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
