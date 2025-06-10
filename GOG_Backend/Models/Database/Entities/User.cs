using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GOG_Backend.Models.Database.Entities
{
    public class User
    {
        [Key]
        public int UsuarioId { get; set; }

        [MaxLength(255)]
        public string NombreUsuario { get; set; }

        [MaxLength(255)]
        public string ContrasenaHash { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Rol { get; set; }

        public int PuntuacionElo { get; set; } = 1200;

        [MaxLength(255)]
        public string? ImagenPerfil { get; set; }

      
    }
}
