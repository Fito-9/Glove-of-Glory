using System.Collections.Concurrent;

namespace GOG_Backend.Services
{
    public class SimpleSessionService
    {
        private readonly ConcurrentDictionary<string, int> _activeSessions = new();

        public string CreateSession(int userId)
        {
            var existingSession = _activeSessions.FirstOrDefault(p => p.Value == userId);
            if (!existingSession.Equals(default(KeyValuePair<string, int>)))
            {
                _activeSessions.TryRemove(existingSession.Key, out _);
            }

            var sessionToken = Guid.NewGuid().ToString();
            _activeSessions[sessionToken] = userId;
            return sessionToken;
        }

        public int? GetUserIdFromSession(string? sessionToken)
        {
            if (string.IsNullOrEmpty(sessionToken)) return null;

            if (_activeSessions.TryGetValue(sessionToken, out var userId))
            {
                return userId;
            }
            return null;
        }

        public void RemoveSession(string? sessionToken)
        {
            if (string.IsNullOrEmpty(sessionToken)) return;
            _activeSessions.TryRemove(sessionToken, out _);
        }
    }
}