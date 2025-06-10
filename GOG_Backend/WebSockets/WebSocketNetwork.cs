using System.Net.WebSockets;
using GOG_Backend.Models.Dto;
using StrategoBackend.WebSockets;
using GOG_Backend.Game;
using System.Text.Json;
using GOG_Backend.Models.Database;
using GOG_Backend.Models.Database.Entities;

namespace GOG_Backend.WebSockets
{
    public class WebSocketNetwork
    {
        private readonly Dictionary<int, WebSocketHandler> _handlers = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Queue<int> _matchmakingQueue = new();
        private readonly Dictionary<string, MatchRoom> _activeRooms = new();
        private readonly IServiceProvider _serviceProvider;

        public WebSocketNetwork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task HandleAsync(WebSocket webSocket, int userId)
        {
            var handler = await AddHandlerAsync(webSocket, userId);
            await BroadcastOnlineUsersAsync();
            await handler.HandleAsync();
        }

        private async Task<WebSocketHandler> AddHandlerAsync(WebSocket webSocket, int userId)
        {
            await _semaphore.WaitAsync();
            var handler = new WebSocketHandler(userId, webSocket);
            handler.MessageReceived += OnMessageReceivedAsync;
            handler.Disconnected += OnDisconnectedAsync;
            _handlers[userId] = handler;
            _semaphore.Release();
            return handler;
        }

        private async Task OnDisconnectedAsync(WebSocketHandler handler)
        {
            await _semaphore.WaitAsync();
            if (_handlers.ContainsKey(handler.UserId))
            {
                _handlers.Remove(handler.UserId);
                RemoveFromMatchmakingQueue(handler.UserId);
            }
            _semaphore.Release();
            await BroadcastOnlineUsersAsync();
        }

        private async Task OnMessageReceivedAsync(WebSocketHandler handler, WebSocketMessageDto message)
        {
            if (message.Type == "matchmakingRequest")
            {
                await HandleMatchmakingRequest(handler);
                return;
            }

            var gameAction = JsonSerializer.Deserialize<GameActionDto>(message.Payload.ToString());
            if (gameAction == null || !_activeRooms.TryGetValue(gameAction.RoomId, out var room))
            {
                return;
            }

            string username = "Unknown";
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var user = await dbContext.Users.FindAsync(handler.UserId);
                if (user != null) username = user.NombreUsuario;
            }

            bool stateChanged = false;

            switch (message.Type)
            {
                case "selectCharacter":
                    var charPayload = JsonSerializer.Deserialize<CharacterSelectionPayload>(gameAction.Payload.ToString());
                    stateChanged = room.SelectCharacter(handler.UserId, charPayload.CharacterName);
                    break;
                case "requestInitialState":
                    if (_activeRooms.TryGetValue(gameAction.RoomId, out var requestedRoom))
                    {
                        // Enviamos el estado actual solo al jugador que lo solicitó
                        var statePayload = requestedRoom.GetStateDto();
                        var stateMessage = new WebSocketMessageDto { Type = "matchStateUpdate", Payload = statePayload };
                        await handler.SendAsync(stateMessage);
                    }
                    break;
                case "banMaps":
                    var banPayload = JsonSerializer.Deserialize<MapBanPayload>(gameAction.Payload.ToString());
                    stateChanged = room.BanMaps(handler.UserId, banPayload.BannedMaps);
                    break;
                case "pickMap":
                    var pickPayload = JsonSerializer.Deserialize<MapPickPayload>(gameAction.Payload.ToString());
                    stateChanged = room.PickMap(handler.UserId, pickPayload.PickedMap);
                    break;
                case "sendChatMessage":
                    var chatPayload = JsonSerializer.Deserialize<ChatMessagePayload>(gameAction.Payload.ToString());
                    room.AddChatMessage(handler.UserId, username, chatPayload.Message);
                    stateChanged = true;
                    break;
                case "declareWinner":
                    var winnerPayload = JsonSerializer.Deserialize<WinnerDeclarationPayload>(gameAction.Payload.ToString());
                    var (isFinished, winnerId, loserId) = room.DeclareWinner(handler.UserId, winnerPayload.DeclaredWinnerId);
                    stateChanged = true;
                    if (isFinished)
                    {
                        await FinalizeMatch(room, winnerId.Value, loserId.Value);
                    }
                    break;
            }

            if (stateChanged)
            {
                await BroadcastRoomState(room.RoomId);
            }
        }

        private async Task HandleMatchmakingRequest(WebSocketHandler handler)
        {
            await _semaphore.WaitAsync();
            if (_matchmakingQueue.Contains(handler.UserId))
            {
                _semaphore.Release();
                return;
            }

            if (_matchmakingQueue.Count > 0)
            {
                int opponentId = _matchmakingQueue.Dequeue();
                if (_handlers.TryGetValue(opponentId, out var opponentHandler))
                {
                    var newRoom = new MatchRoom(opponentId, handler.UserId);
                    _activeRooms[newRoom.RoomId] = newRoom;
                    var matchMessage = new WebSocketMessageDto { Type = "matchFound", Payload = new { roomId = newRoom.RoomId } };

                    // Notificamos a ambos jugadores que se encontró una partida
                    await opponentHandler.SendAsync(matchMessage);
                    await handler.SendAsync(matchMessage);
                    await BroadcastRoomState(newRoom.RoomId);
                }
            }
            else
            {
                _matchmakingQueue.Enqueue(handler.UserId);
                await handler.SendAsync(new WebSocketMessageDto { Type = "waitingForMatch", Payload = "Esperando oponente..." });
            }
            _semaphore.Release();
        }

        private async Task BroadcastRoomState(string roomId)
        {
            if (_activeRooms.TryGetValue(roomId, out var room))
            {
                var statePayload = room.GetStateDto();
                var message = new WebSocketMessageDto { Type = "matchStateUpdate", Payload = statePayload };
                if (_handlers.TryGetValue(room.Player1Id, out var p1Handler)) await p1Handler.SendAsync(message);
                if (_handlers.TryGetValue(room.Player2Id, out var p2Handler)) await p2Handler.SendAsync(message);
            }
        }

        private async Task FinalizeMatch(MatchRoom room, int winnerId, int loserId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var winner = await dbContext.Users.FindAsync(winnerId);
                var loser = await dbContext.Users.FindAsync(loserId);

                if (winner != null && loser != null)
                {
                    winner.PuntuacionElo += 20;
                    loser.PuntuacionElo = Math.Max(0, loser.PuntuacionElo - 10);

                    var newMatch = new Match
                    {
                        MatchId = Guid.Parse(room.RoomId),
                        Player1Id = room.Player1Id,
                        Player2Id = room.Player2Id,
                        WinnerId = winnerId,
                        Player1Character = room.Player1Character,
                        Player2Character = room.Player2Character,
                        Map = room.SelectedMap
                    };
                    await dbContext.Matches.AddAsync(newMatch);
                    await dbContext.SaveChangesAsync();
                }
            }
            _activeRooms.Remove(room.RoomId);
        }

        // --- Métodos auxiliares que ya tenías ---
        private void RemoveFromMatchmakingQueue(int userId) { /* ... */ }
        private async Task BroadcastOnlineUsersAsync() { /* ... */ }
        public List<int> GetConnectedUsers() => _handlers.Keys.ToList();
    }
}