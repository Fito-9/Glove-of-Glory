using System.ComponentModel.DataAnnotations;

namespace GOG_Backend.Models.Dto
{
    public class UserFullProfileDto : UserProfileDto
    {
        public string Email { get; set; }
    }
}
