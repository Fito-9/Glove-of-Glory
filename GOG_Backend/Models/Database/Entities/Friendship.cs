using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GOG_Backend.Models.Database.Entities
{
    public class Friendship
    {
        [Key]
        public int FriendshipId { get; set; }

        public int SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User Sender { get; set; }

        public int ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public User Receiver { get; set; }

        public bool IsAccepted { get; set; } = false;
    }
}
