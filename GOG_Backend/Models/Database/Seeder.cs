using GOG_Backend.Models.Database.Entities;
using GOG_Backend.Utils;

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
                    ContrasenaHash = PasswordHelper.Hash("contraseña"), 
                    Email = "gonza@example.com",
                    Rol = "Admin",
                    PuntuacionElo = 1200,
                    ImagenPerfil = ""
                }
            ];

            _context.Users.AddRange(users);
            _context.SaveChanges();
        }
    }
}
