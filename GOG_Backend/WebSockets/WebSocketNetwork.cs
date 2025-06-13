using System.Net.WebSockets;
using GOG_Backend.Models.Dto;
using StrategoBackend.WebSockets;
using GOG_Backend.Game;
using System.Text.Json;
using GOG_Backend.Models.Database;
using GOG_Backend.Models.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            if (userId == 0)
            {
                Console.WriteLine("Error: Se intentó conectar un usuario con ID 0. Conexión rechazada.");
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Token inválido o ausente.", CancellationToken.None);
                return;
            }

            var handler = await AddHandlerAsync(webSocket, userId);
            await BroadcastOnlineUsersAsync();
            await handler.HandleAsync();
        }

        private async Task<WebSocketHandler> AddHandlerAsync(WebSocket webSocket, int userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                var handler = new WebSocketHandler(userId, webSocket);
                handler.MessageReceived += OnMessageReceivedAsync;
                handler.Disconnected += OnDisconnectedAsync;
                _handlers[userId] = handler;
                return handler;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task OnDisconnectedAsync(WebSocketHandler handler)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_handlers.ContainsKey(handler.UserId))
                {
                    _handlers.Remove(handler.UserId);
                    RemoveFromMatchmakingQueue(handler.UserId);
                }
            }
            finally
            {
                _semaphore.Release();
            }
            await BroadcastOnlineUsersAsync();
        }

        // ✅✅✅ INICIO DE LA REFACTORIZACIÓN CRÍTICA DEL BACKEND ✅✅✅
        private async Task OnMessageReceivedAsync(WebSocketHandler handler, WebSocketMessageDto message)
        {
            Console.WriteLine($"Mensaje recibido de {handler.UserId}: Tipo={message.Type}, Payload={message.Payload}");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var payloadElement = (JsonElement)message.Payload;

            // Los mensajes que no son de juego se manejan fuera del semáforo para agilidad
            switch (message.Type)
            {
                case "matchmakingRequest":
                    await HandleMatchmakingRequest(handler);
                    return;
                case "inviteFriend":
                    var invitePayload = payloadElement.Deserialize<InviteFriendPayload>(options);
                    if (invitePayload != null)
                    {
                        await HandleFriendInvite(handler.UserId, invitePayload.InvitedUserId);
                    }
                    return;
            }

            // A partir de aquí, todas las acciones de juego se protegen con el semáforo.
            await _semaphore.WaitAsync();
            try
            {
                bool stateChanged = false;
                MatchRoom room = null;
                string roomId = null;

                // Extraemos el RoomId del payload de la acción
                if (payloadElement.TryGetProperty("RoomId", out var roomIdElement))
                {
                    roomId = roomIdElement.GetString();
                    if (roomId != null && !_activeRooms.TryGetValue(roomId, out room))
                    {
                        Console.WriteLine($"Acción para una sala no existente o inválida: {roomId}");
                        return;
                    }
                }

                string username = "Unknown";
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                    var user = await dbContext.Users.FindAsync(handler.UserId);
                    if (user != null) username = user.NombreUsuario;
                }

                switch (message.Type)
                {
                    case "selectCharacter":
                        var charPayload = payloadElement.Deserialize<CharacterSelectionPayload>(options);
                        if (room != null && charPayload != null) stateChanged = room.SelectCharacter(handler.UserId, charPayload.CharacterName);
                        break;
                    case "banMaps":
                        var banPayload = payloadElement.Deserialize<MapBanPayload>(options);
                        if (room != null && banPayload != null) stateChanged = room.BanMaps(handler.UserId, banPayload.BannedMaps);
                        break;
                    case "pickMap":
                        var pickPayload = payloadElement.Deserialize<MapPickPayload>(options);
                        if (room != null && pickPayload != null) stateChanged = room.PickMap(handler.UserId, pickPayload.PickedMap);
                        break;
                    case "sendChatMessage":
                        var chatPayload = payloadElement.Deserialize<ChatMessagePayload>(options);
                        if (room != null && chatPayload != null)
                        {
                            room.AddChatMessage(handler.UserId, username, chatPayload.Message);
                            stateChanged = true;
                        }
                        break;
                    case "declareWinner":
                        var winnerPayload = payloadElement.Deserialize<WinnerDeclarationPayload>(options);
                        if (room != null && winnerPayload != null)
                        {
                            var (isFinished, players, voteMismatch) = room.DeclareWinner(handler.UserId, winnerPayload.DeclaredWinnerId);
                            stateChanged = true; // Siempre actualizamos para reflejar el voto

                            if (voteMismatch)
                            {
                                var mismatchMessage = new WebSocketMessageDto { Type = "voteMismatch", Payload = new { roomId = room.RoomId } };
                                if (_handlers.TryGetValue(room.Player1Id, out var p1Handler)) await p1Handler.SendAsync(mismatchMessage);
                                if (_handlers.TryGetValue(room.Player2Id, out var p2Handler)) await p2Handler.SendAsync(mismatchMessage);
                            }

                            if (isFinished && players.winner.HasValue && players.loser.HasValue)
                            {
                                await FinalizeMatch(room, players.winner.Value, players.loser.Value);
                                // No se necesita broadcast, la sala se elimina
                            }
                        }
                        break;
                    case "requestInitialState":
                        var requestPayload = payloadElement.Deserialize<RequestInitialStatePayload>(options);
                        if (requestPayload != null && _activeRooms.TryGetValue(requestPayload.RoomId, out var requestedRoom))
                        {
                            var statePayload = requestedRoom.GetStateDto();
                            var stateMessage = new WebSocketMessageDto { Type = "matchStateUpdate", Payload = statePayload };
                            await handler.SendAsync(stateMessage);
                        }
                        break;
                    default:
                        Console.WriteLine($"Tipo de mensaje no reconocido recibido: {message.Type}");
                        break;
                }

                if (stateChanged && roomId != null && _activeRooms.ContainsKey(roomId))
                {
                    await BroadcastRoomState(roomId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando mensaje '{message.Type}': {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        // ✅✅✅ FIN DE LA REFACTORIZACIÓN CRÍTICA DEL BACKEND ✅✅✅

        private async Task HandleFriendInvite(int senderId, int receiverId)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_handlers.TryGetValue(receiverId, out var receiverHandler))
                {
                    string senderUsername, receiverUsername;
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        var user1 = await dbContext.Users.FindAsync(senderId);
                        var user2 = await dbContext.Users.FindAsync(receiverId);
                        senderUsername = user1?.NombreUsuario ?? $"Usuario (ID: {senderId})";
                        receiverUsername = user2?.NombreUsuario ?? $"Usuario (ID: {receiverId})";
                    }

                    var newRoom = new MatchRoom(senderId, senderUsername, receiverId, receiverUsername, isRanked: false);
                    _activeRooms[newRoom.RoomId] = newRoom;

                    var matchMessage = new WebSocketMessageDto { Type = "matchFound", Payload = new { roomId = newRoom.RoomId } };

                    if (_handlers.TryGetValue(senderId, out var senderHandler))
                    {
                        await senderHandler.SendAsync(matchMessage);
                    }
                    await receiverHandler.SendAsync(matchMessage);

                    await BroadcastRoomState(newRoom.RoomId);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task HandleMatchmakingRequest(WebSocketHandler handler)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_matchmakingQueue.Contains(handler.UserId) || _activeRooms.Values.Any(r => r.Player1Id == handler.UserId || r.Player2Id == handler.UserId))
                {
                    return;
                }

                if (_matchmakingQueue.Count > 0)
                {
                    int opponentId = _matchmakingQueue.Dequeue();
                    if (_handlers.TryGetValue(opponentId, out var opponentHandler))
                    {
                        string player1Username, player2Username;
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                            var user1 = await dbContext.Users.FindAsync(opponentId);
                            var user2 = await dbContext.Users.FindAsync(handler.UserId);
                            player1Username = user1?.NombreUsuario ?? $"Usuario (ID: {opponentId})";
                            player2Username = user2?.NombreUsuario ?? $"Usuario (ID: {handler.UserId})";
                        }

                        var newRoom = new MatchRoom(opponentId, player1Username, handler.UserId, player2Username, isRanked: true);
                        _activeRooms[newRoom.RoomId] = newRoom;

                        var matchMessage = new WebSocketMessageDto { Type = "matchFound", Payload = new { roomId = newRoom.RoomId } };

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
            }
            finally
            {
                _semaphore.Release();
            }
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
                    if (room.IsRanked)
                    {
                        winner.PuntuacionElo += 15;
                        loser.PuntuacionElo = Math.Max(0, loser.PuntuacionElo - 15);
                    }

                    var newMatch = new Match
                    {
                        MatchId = Guid.Parse(room.RoomId),
                        Player1Id = room.Player1Id,
                        Player2Id = room.Player2Id,
                        WinnerId = winnerId,
                        Player1Character = room.Player1Character,
                        Player2Character = room.Player2Character,
                        Map = room.SelectedMap,
                        IsRanked = room.IsRanked
                    };
                    await dbContext.Matches.AddAsync(newMatch);
                    await dbContext.SaveChangesAsync();
                }
            }
            _activeRooms.Remove(room.RoomId);
        }

        private void RemoveFromMatchmakingQueue(int userId)
        {
            if (_matchmakingQueue.Contains(userId))
            {
                var newQueue = new Queue<int>(_matchmakingQueue.Where(id => id != userId));
                _matchmakingQueue.Clear();
                foreach (var id in newQueue)
                {
                    _matchmakingQueue.Enqueue(id);
                }
            }
        }

        private async Task BroadcastOnlineUsersAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var onlineUserIds = _handlers.Keys.ToList();
                var dto = new WebSocketMessageDto
                {
                    Type = "onlineUsers",
                    Payload = onlineUserIds
                };
                var tasks = _handlers.Values.Select(handler => handler.SendAsync(dto));
                await Task.WhenAll(tasks);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public List<int> GetConnectedUsers() => _handlers.Keys.ToList();
    }
}