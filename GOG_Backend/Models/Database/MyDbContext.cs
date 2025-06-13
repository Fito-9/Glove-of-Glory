using GOG_Backend.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace GOG_Backend.Models.Database
{
    public class MyDbContext : DbContext
    {
        private const string DATABASE_PATH = "GOG.db";

        // Cada DbSet es una tabla en la base de datos.
        public DbSet<User> Users { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        // Le decimos que use un archivo SQLite como base de datos.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            optionsBuilder.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
        }

        // Aquí definimos las relaciones entre tablas.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones para la tabla Match.
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player1)
                .WithMany()
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict); // No dejar borrar un usuario si tiene partidas.

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player2)
                .WithMany()
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Winner)
                .WithMany()
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación de amistad (Muchos a Muchos) a través de la tabla Friendship.
            modelBuilder.Entity<Friendship>()
              .HasOne(f => f.Sender)
              .WithMany()
              .HasForeignKey(f => f.SenderId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}