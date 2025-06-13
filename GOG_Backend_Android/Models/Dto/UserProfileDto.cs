using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GOG_Backend.Models.Dto
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public int Elo { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
