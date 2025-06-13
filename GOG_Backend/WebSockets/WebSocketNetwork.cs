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
    // El cerebro de todo el tinglado en tiempo real.
    // Controla quién está conectado, las colas de matchmaking y las salas de juego.
    public class WebSocketNetwork
    {
        // Diccionarios para guardar el estado de todo.
        private readonly Dictionary<int, WebSocketHandler> _handlers = new(); // Guarda la conexión de cada usuario.
        private readonly SemaphoreSlim _semaphore = new(1, 1); // Un semáforo para que no se pisen los hilos al modificar los diccionarios.
        private readonly Queue<int> _matchmakingQueue = new(); // La cola de gente esperando partida.
        private readonly Dictionary<string, MatchRoom> _activeRooms = new(); // Las salas de juego que están en marcha.
        private readonly IServiceProvider _serviceProvider;

        public WebSocketNetwork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Cuando llega una nueva conexión WebSocket, esto es lo primero que se ejecuta.
        public async Task HandleAsync(WebSocket webSocket, int userId)
        {
            if (userId == 0)
            {
                // Si no hay ID de usuario, es que el token es malo. Puerta.
                Console.WriteLine("Error: Usuario con ID 0 intentando conectar. Conexión rechazada.");
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Token inválido o ausente.", CancellationToken.None);
                return;
            }

            var handler = await AddHandlerAsync(webSocket, userId);
            await BroadcastOnlineUsersAsync(); // Avisamos a todos que ha entrado alguien nuevo.
            await handler.HandleAsync(); // Ponemos al handler a escuchar mensajes.
        }

        // Mete a un nuevo usuario en la lista de conectados.
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

        // Se ejecuta cuando un usuario se desconecta. Limpieza.
        private async Task OnDisconnectedAsync(WebSocketHandler handler)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_handlers.ContainsKey(handler.UserId))
                {
                    _handlers.Remove(handler.UserId);
                    RemoveFromMatchmakingQueue(handler.UserId); // Lo sacamos de la cola si estaba buscando.
                }
            }
            finally
            {
                _semaphore.Release();
            }
            await BroadcastOnlineUsersAsync(); // Avisamos a todos que alguien se ha pirado.
        }

        // El "router" de los mensajes WebSocket. Decide qué hacer con cada mensaje que llega.
        private async Task OnMessageReceivedAsync(WebSocketHandler handler, WebSocketMessageDto message)
        {
            Console.WriteLine($"Mensaje recibido de {handler.UserId}: Tipo={message.Type}");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var payloadElement = (JsonElement)message.Payload;

            // Primero miramos los mensajes que no son de una sala específica.
            switch (message.Type)
            {
                case "matchmakingRequest":
                    await HandleMatchmakingRequest(handler);
                    return;
                case "inviteFriend":
                    var invitePayload = payloadElement.Deserialize<InviteFriendPayload>(options);
                    if (invitePayload != null) await HandleFriendInvite(handler.UserId, invitePayload.InvitedUserId);
                    return;
                case "acceptInvite":
                    var acceptPayload = payloadElement.Deserialize<InviteFriendPayload>(options);
                    if (acceptPayload != null) await HandleAcceptInvite(handler.UserId, acceptPayload.InvitedUserId);
                    return;
            }

            // Si el mensaje es para una sala de juego, lo gestionamos aquí.
            await _semaphore.WaitAsync();
            try
            {
                string roomId = payloadElement.TryGetProperty("RoomId", out var roomIdElement) ? roomIdElement.GetString() : null;
                if (roomId == null || !_activeRooms.TryGetValue(roomId, out var room))
                {
                    Console.WriteLine($"Acción para una sala que no existe: {roomId}");
                    return;
                }

                // Pillamos el nombre de usuario de la BD para el chat.
                string username = "Desconocido";
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                    var user = await dbContext.Users.FindAsync(handler.UserId);
                    if (user != null) username = user.NombreUsuario;
                }

                bool stateChanged = false;

                // Acciones dentro de una partida.
                switch (message.Type)
                {
                    case "selectCharacter":
                        var charPayload = payloadElement.Deserialize<CharacterSelectionPayload>(options);
                        if (charPayload != null) stateChanged = room.SelectCharacter(handler.UserId, charPayload.CharacterName);
                        break;
                    case "banMaps":
                        var banPayload = payloadElement.Deserialize<MapBanPayload>(options);
                        if (banPayload != null) stateChanged = room.BanMaps(handler.UserId, banPayload.BannedMaps);
                        break;
                    case "pickMap":
                        var pickPayload = payloadElement.Deserialize<MapPickPayload>(options);
                        if (pickPayload != null) stateChanged = room.PickMap(handler.UserId, pickPayload.PickedMap);
                        break;
                    case "sendChatMessage":
                        var chatPayload = payloadElement.Deserialize<ChatMessagePayload>(options);
                        if (chatPayload != null)
                        {
                            room.AddChatMessage(handler.UserId, username, chatPayload.Message);
                            stateChanged = true;
                        }
                        break;
                    case "declareWinner":
                        var winnerPayload = payloadElement.Deserialize<WinnerDeclarationPayload>(options);
                        if (winnerPayload != null)
                        {
                            var (isFinished, players, voteMismatch) = room.DeclareWinner(handler.UserId, winnerPayload.DeclaredWinnerId);
                            stateChanged = true;

                            if (voteMismatch)
                            {
                                var mismatchMessage = new WebSocketMessageDto { Type = "voteMismatch", Payload = new { roomId = room.RoomId } };
                                if (_handlers.TryGetValue(room.Player1Id, out var p1Handler)) await p1Handler.SendAsync(mismatchMessage);
                                if (_handlers.TryGetValue(room.Player2Id, out var p2Handler)) await p2Handler.SendAsync(mismatchMessage);
                            }

                            if (isFinished && players.winner.HasValue)
                            {
                                await BroadcastRoomState(roomId);
                                await FinalizeMatch(room, players.winner.Value, players.loser.Value);
                                _activeRooms.Remove(roomId); // La partida ha terminado, la eliminamos.
                                stateChanged = false;
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
                }

                if (stateChanged)
                {
                    await BroadcastRoomState(roomId); // Si algo cambió, avisamos a los jugadores de la sala.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando mensaje '{message.Type}': {ex.Message} \n {ex.StackTrace}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Manda una invitación de partida a un amigo.
        private async Task HandleFriendInvite(int senderId, int receiverId)
        {
            if (_handlers.TryGetValue(receiverId, out var receiverHandler))
            {
                string senderUsername;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                    var sender = await dbContext.Users.FindAsync(senderId);
                    senderUsername = sender?.NombreUsuario ?? $"Usuario (ID: {senderId})";
                }

                var inviteMessage = new WebSocketMessageDto
                {
                    Type = "gameInviteReceived",
                    Payload = new { inviterId = senderId, inviterName = senderUsername }
                };

                await receiverHandler.SendAsync(inviteMessage);
                Console.WriteLine($"Invitación enviada de {senderId} a {receiverId}");
            }
            else
            {
                Console.WriteLine($"No se pudo enviar invitación a {receiverId} porque no está conectado.");
            }
        }

        // Cuando alguien acepta una invitación, se crea la sala.
        private async Task HandleAcceptInvite(int receiverId, int senderId)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_handlers.TryGetValue(senderId, out var senderHandler) && _handlers.TryGetValue(receiverId, out var receiverHandler))
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

                    await senderHandler.SendAsync(matchMessage);
                    await receiverHandler.SendAsync(matchMessage);

                    Console.WriteLine($"Partida creada entre {senderId} y {receiverId} en la sala {newRoom.RoomId}");

                    await BroadcastRoomState(newRoom.RoomId);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Gestiona la cola de matchmaking.
        private async Task HandleMatchmakingRequest(WebSocketHandler handler)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_matchmakingQueue.Contains(handler.UserId) || _activeRooms.Values.Any(r => r.Player1Id == handler.UserId || r.Player2Id == handler.UserId))
                {
                    return; // Si ya está en cola o en partida, no hace nada.
                }

                if (_matchmakingQueue.Count > 0)
                {
                    // Si hay alguien esperando, los emparejamos.
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
                    // Si no hay nadie, lo ponemos a la cola.
                    _matchmakingQueue.Enqueue(handler.UserId);
                    await handler.SendAsync(new WebSocketMessageDto { Type = "waitingForMatch", Payload = "Esperando oponente..." });
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Envía el estado actual de una sala a sus dos jugadores.
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

        // Cuando la partida termina, actualiza el ELO y guarda el resultado.
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

                    // Avisa a los jugadores de su nuevo ELO.
                    var winnerUpdatePayload = new { newElo = winner.PuntuacionElo };
                    var loserUpdatePayload = new { newElo = loser.PuntuacionElo };

                    var winnerMessage = new WebSocketMessageDto { Type = "eloUpdate", Payload = winnerUpdatePayload };
                    var loserMessage = new WebSocketMessageDto { Type = "eloUpdate", Payload = loserUpdatePayload };

                    if (_handlers.TryGetValue(winnerId, out var winnerHandler))
                    {
                        await winnerHandler.SendAsync(winnerMessage);
                    }
                    if (_handlers.TryGetValue(loserId, out var loserHandler))
                    {
                        await loserHandler.SendAsync(loserMessage);
                    }
                }
            }
        }

        // Saca a un usuario de la cola de matchmaking.
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

        // Envía la lista de usuarios conectados a todos.
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