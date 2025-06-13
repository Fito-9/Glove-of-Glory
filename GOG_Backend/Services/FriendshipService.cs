using GOG_Backend.Models.Database.Entities;
using GOG_Backend.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace GOG_Backend.Services
{
    public class FriendshipService
    {
        private readonly MyDbContext _dbContext;

        public FriendshipService(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendFriendRequest(int senderId, int receiverId)
        {
            // Evitar duplicados
            var existingRequest = await _dbContext.Friendships
                .AnyAsync(f => (f.SenderId == senderId && f.ReceiverId == receiverId) || (f.SenderId == receiverId && f.ReceiverId == senderId));

            if (existingRequest) return;

            var friendship = new Friendship
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                IsAccepted = false
            };
            await _dbContext.Friendships.AddAsync(friendship);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AcceptFriendRequest(int senderId, int receiverId)
        {
            var friendship = await _dbContext.Friendships
                .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == receiverId && !f.IsAccepted);
            if (friendship != null)
            {
                friendship.IsAccepted = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RejectFriendRequest(int senderId, int receiverId)
        {
            var friendship = await _dbContext.Friendships
                .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == receiverId && !f.IsAccepted);
            if (friendship != null)
            {
                _dbContext.Friendships.Remove(friendship);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetFriends(int userId)
        {
            var friendIds = await _dbContext.Friendships
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.IsAccepted)
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToListAsync();

            return await _dbContext.Users.Where(u => friendIds.Contains(u.UsuarioId)).ToListAsync();
        }

        public async Task<List<Friendship>> GetPendingRequests(int userId)
        {
            return await _dbContext.Friendships
                .Include(f => f.Sender) // Incluir datos del remitente
                .Where(f => f.ReceiverId == userId && !f.IsAccepted)
                .ToListAsync();
        }
    }
}
