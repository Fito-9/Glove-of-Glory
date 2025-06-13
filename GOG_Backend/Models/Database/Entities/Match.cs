using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GOG_Backend.Models.Database.Entities
{
    public class Match
    {
        [Key]
        public Guid MatchId { get; set; }

        public int Player1Id { get; set; }
        [ForeignKey("Player1Id")]
        public User Player1 { get; set; }

        public int Player2Id { get; set; }
        [ForeignKey("Player2Id")]
        public User Player2 { get; set; }

        public int WinnerId { get; set; }
        [ForeignKey("WinnerId")]
        public User Winner { get; set; }

        public string Player1Character { get; set; }
        public string Player2Character { get; set; }
        public string Map { get; set; }
        public bool IsRanked { get; set; }
    }
}