namespace GOG_Backend.Models.Dto
{
    // ✅ DTO para todas las peticiones del admin
    public class AdminRequestDto
    {
        public string AdminToken { get; set; }
    }

    // DTO para mostrar la lista de usuarios en el panel de admin
    public class AdminUserViewDto
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public int PuntuacionElo { get; set; }
        public string Rol { get; set; }
    }

    // DTO para recibir los datos al actualizar un usuario
    public class AdminUpdateUserDto : AdminRequestDto
    {
        public string Nickname { get; set; }
        public int PuntuacionElo { get; set; }
        public string Rol { get; set; }
    }
}