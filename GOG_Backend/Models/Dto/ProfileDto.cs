namespace GOG_Backend.Models.Dto
{
    public class UserProfileDto
    {
        public string Nickname { get; set; }
        public string Email { get; set; }
        public int PuntuacionElo { get; set; }
        public string AvatarUrl { get; set; }
    }

    // DTO para recibir los datos al actualizar el perfil
    public class UpdateProfileDto
    {
        public string AdminToken { get; set; } 
        public string Nickname { get; set; }
        // No incluimos el ELO para que no se pueda editar
    }
}
