using GOG_Backend.Models.Database.Entities;

namespace GOG_Backend.Models.Database
{
    public class Seeder
    {
        private readonly MyDbContext _context;

        public Seeder(MyDbContext dbContext)
        {
            _context = dbContext;
        }

        public void Seed()
        {
            User[] users =
            [
                new User
                {
                    NombreUsuario = "gonza",
                    ContrasenaHash = "contraseña", 
                    Email = "gonza@example.com",
                    Rol = "Jugador",
                    PuntuacionElo = 1200,
                    ImagenPerfil = ""
                }
            ];

            _context.Users.AddRange(users);
            _context.SaveChanges();
        }
    }
}
