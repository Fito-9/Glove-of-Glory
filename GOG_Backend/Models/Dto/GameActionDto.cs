namespace GOG_Backend.Models.Dto
{
    public class GameActionDto
    {
        public string RoomId { get; set; }
        public object Payload { get; set; }
    }

    public class CharacterSelectionPayload
    {
        public string CharacterName { get; set; }
    }

    public class MapBanPayload
    {
        public List<string> BannedMaps { get; set; }
    }

    public class MapPickPayload
    {
        public string PickedMap { get; set; }
    }

    public class ChatMessagePayload
    {
        public string Message { get; set; }
    }

    public class WinnerDeclarationPayload
    {
        public int DeclaredWinnerId { get; set; }
    }
    public class InviteFriendPayload
    {
        public int InvitedUserId { get; set; }
    }

    public class RequestInitialStatePayload
    {
        public string RoomId { get; set; }
    }
}
