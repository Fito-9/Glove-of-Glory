using GOG_Backend.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace GOG_Backend.Models.Database
{
    public class MyDbContext : DbContext
    {
        private const string DATABASE_PATH = "GOG.db";

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            optionsBuilder.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
        }
    }
}
