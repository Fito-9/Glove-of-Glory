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

            GameActionDto gameActionPayload;
            try
            {

                gameActionPayload = JsonSerializer.Deserialize<GameActionDto>(message.Payload.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                Console.WriteLine($"Usuario {handler.UserId} envió un payload no válido que no es un GameActionDto.");
                return;
            }

            if (gameActionPayload == null || string.IsNullOrEmpty(gameActionPayload.RoomId) || !_activeRooms.TryGetValue(gameActionPayload.RoomId, out var room))
            {
                Console.WriteLine($"Acción para una sala no existente o inválida: {gameActionPayload?.RoomId}");
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
            try
            {
                var actionPayloadElement = (JsonElement)gameActionPayload.Payload;
                switch (message.Type)
                {
                    case "selectCharacter":
                        var charPayload = actionPayloadElement.Deserialize<CharacterSelectionPayload>();
                        if (charPayload != null) stateChanged = room.SelectCharacter(handler.UserId, charPayload.CharacterName);
                        break;
                    case "banMaps":
                        var banPayload = actionPayloadElement.Deserialize<MapBanPayload>();
                        if (banPayload != null) stateChanged = room.BanMaps(handler.UserId, banPayload.BannedMaps);
                        break;
                    case "pickMap":
                        var pickPayload = actionPayloadElement.Deserialize<MapPickPayload>();
                        if (pickPayload != null) stateChanged = room.PickMap(handler.UserId, pickPayload.PickedMap);
                        break;
                    case "sendChatMessage":
                        var chatPayload = actionPayloadElement.Deserialize<ChatMessagePayload>();
                        if (chatPayload != null)
                        {
                            room.AddChatMessage(handler.UserId, username, chatPayload.Message);
                            stateChanged = true;
                        }
                        break;
                    case "declareWinner":
                        var winnerPayload = actionPayloadElement.Deserialize<WinnerDeclarationPayload>();
                        if (winnerPayload != null)
                        {
                            var (isFinished, winnerId, loserId) = room.DeclareWinner(handler.UserId, winnerPayload.DeclaredWinnerId);
                            stateChanged = true;
                            if (isFinished && winnerId.HasValue && loserId.HasValue)
                            {
                                await FinalizeMatch(room, winnerId.Value, loserId.Value);
                            }
                        }
                        break;
                    case "inviteFriend":
                        var invitePayload = actionPayloadElement.Deserialize<InviteFriendPayload>();
                        if (invitePayload != null)
                        {
                            await HandleFriendInvite(handler.UserId, invitePayload.InvitedUserId);
                        }
                        break;
                    case "requestInitialState":
                        if (_activeRooms.TryGetValue(gameActionPayload.RoomId, out var requestedRoom))
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando acción '{message.Type}' para la sala {room.RoomId}: {ex.Message}");
            }

            if (stateChanged)
            {
                await BroadcastRoomState(room.RoomId);
            }
        }

        private async Task HandleFriendInvite(int senderId, int receiverId)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_handlers.TryGetValue(receiverId, out var receiverHandler))
                {
   
                    string senderUsername = "Player1";
                    string receiverUsername = "Player2";
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        var user1 = await dbContext.Users.FindAsync(senderId);
                        var user2 = await dbContext.Users.FindAsync(receiverId);
                        if (user1 != null) senderUsername = user1.NombreUsuario;
                        if (user2 != null) receiverUsername = user2.NombreUsuario;
                    }

                    var newRoom = new MatchRoom(senderId, senderUsername, receiverId, receiverUsername);
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
                    string player1Username = "Player1";
                    string player2Username = "Player2";
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        var user1 = await dbContext.Users.FindAsync(opponentId);
                        var user2 = await dbContext.Users.FindAsync(handler.UserId);
                        if (user1 != null) player1Username = user1.NombreUsuario;
                        if (user2 != null) player2Username = user2.NombreUsuario;
                    }

                    var newRoom = new MatchRoom(opponentId, player1Username, handler.UserId, player2Username);
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
            var onlineUserIds = _handlers.Keys.ToList();
            var dto = new WebSocketMessageDto
            {
                Type = "onlineUsers",
                Payload = onlineUserIds
            };

            await _semaphore.WaitAsync();
            try
            {
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