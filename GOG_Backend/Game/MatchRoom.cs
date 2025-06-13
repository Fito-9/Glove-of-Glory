using System;
using System.Collections.Generic;

namespace GOG_Backend.Game
{
    // Las fases por las que pasa una partida.
    public enum GameState
    {
        WaitingForPlayers,
        CharacterSelection,
        MapBanP1,
        MapBanP2,
        MapPickP1,
        ReadyToStart,
        WinnerDeclaration,
        Finished
    }

    // Esta clase representa una sala de partida y guarda todo su estado.
    public class MatchRoom
    {
        public string RoomId { get; }
        public int Player1Id { get; }
        public int Player2Id { get; }
        public GameState CurrentState { get; private set; }
        public bool IsRanked { get; }

        public string Player1Username { get; }
        public string Player2Username { get; }

        public string Player1Character { get; private set; }
        public string Player2Character { get; private set; }

        public List<string> MapPool { get; private set; }
        public List<string> BannedMaps { get; private set; } = new List<string>();
        public string SelectedMap { get; private set; }

        public List<object> ChatMessages { get; } = new List<object>();

        // Votos de los jugadores para decidir el ganador.
        private int? _player1WinnerVote;
        private int? _player2WinnerVote;

        // Constructor: se llama al crear una nueva sala.
        public MatchRoom(int player1Id, string player1Username, int player2Id, string player2Username, bool isRanked)
        {
            RoomId = Guid.NewGuid().ToString();
            Player1Id = player1Id;
            Player1Username = player1Username;
            Player2Id = player2Id;
            Player2Username = player2Username;
            IsRanked = isRanked;
            CurrentState = GameState.CharacterSelection; // La partida empieza en selección de personaje.
            MapPool = new List<string> { "Small Battlefield", "Battlefield", "Final Destination", "Pokémon Stadium 2", "Hollow Bastion", "Smashville", "Town and City", "Kalos Pokémon League", "Yoshi's Story" };
        }

        // Un jugador elige su personaje.
        public bool SelectCharacter(int userId, string character)
        {
            if (CurrentState != GameState.CharacterSelection) return false;

            if (userId == Player1Id) Player1Character = character;
            else if (userId == Player2Id) Player2Character = character;
            else return false;

            // Si ambos han elegido, pasamos a la siguiente fase.
            if (!string.IsNullOrEmpty(Player1Character) && !string.IsNullOrEmpty(Player2Character))
            {
                CurrentState = GameState.MapBanP1;
            }
            return true;
        }

        // Un jugador banea mapas.
        public bool BanMaps(int userId, List<string> mapsToBan)
        {
            if ((userId == Player1Id && CurrentState != GameState.MapBanP1) ||
                (userId == Player2Id && CurrentState != GameState.MapBanP2))
            {
                return false; // No es tu turno, fuera.
            }

            BannedMaps.AddRange(mapsToBan);
            MapPool.RemoveAll(m => mapsToBan.Contains(m));

            // Pasa el turno al siguiente jugador o a la selección de mapa.
            if (CurrentState == GameState.MapBanP1) CurrentState = GameState.MapBanP2;
            else if (CurrentState == GameState.MapBanP2) CurrentState = GameState.MapPickP1;

            return true;
        }

        // El jugador 1 elige el mapa final.
        public bool PickMap(int userId, string map)
        {
            if (userId != Player1Id || CurrentState != GameState.MapPickP1) return false;

            SelectedMap = map;
            CurrentState = GameState.WinnerDeclaration; // A jugar!
            return true;
        }

        // Añade un mensaje al chat de la sala.
        public void AddChatMessage(int userId, string username, string message)
        {
            ChatMessages.Add(new { userId, username, message, timestamp = DateTime.UtcNow });
        }

        // Gestiona la votación del ganador.
        public (bool isFinished, (int? winner, int? loser) players, bool voteMismatch) DeclareWinner(int userId, int declaredWinnerId)
        {
            if (CurrentState != GameState.WinnerDeclaration) return (false, (null, null), false);

            if (userId == Player1Id) _player1WinnerVote = declaredWinnerId;
            else if (userId == Player2Id) _player2WinnerVote = declaredWinnerId;

            // Si los votos no coinciden, se resetea la votación.
            if (_player1WinnerVote.HasValue && _player2WinnerVote.HasValue && _player1WinnerVote != _player2WinnerVote)
            {
                _player1WinnerVote = null;
                _player2WinnerVote = null;
                return (false, (null, null), true); // Hubo discrepancia.
            }

            // Si ambos votan lo mismo, la partida termina.
            if (_player1WinnerVote.HasValue && _player1WinnerVote == _player2WinnerVote)
            {
                CurrentState = GameState.Finished;
                int winner = _player1WinnerVote.Value;
                int loser = (winner == Player1Id) ? Player2Id : Player1Id;
                return (true, (winner, loser), false); // Partida finalizada.
            }

            return (false, (null, null), false); // Aún falta un voto.
        }

        // Prepara un objeto con el estado actual de la sala para enviarlo al frontend.
        public object GetStateDto()
        {
            return new
            {
                roomId = RoomId,
                currentState = CurrentState.ToString(),
                player1Id = Player1Id,
                player1Username = Player1Username,
                player2Id = Player2Id,
                player2Username = Player2Username,
                player1Character = (CurrentState > GameState.CharacterSelection) ? Player1Character : null,
                player2Character = (CurrentState > GameState.CharacterSelection) ? Player2Character : null,
                mapPool = MapPool,
                bannedMaps = BannedMaps,
                selectedMap = SelectedMap,
                chatMessages = ChatMessages,
                player1Voted = _player1WinnerVote.HasValue,
                player2Voted = _player2WinnerVote.HasValue
            };
        }
    }
}