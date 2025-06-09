namespace GOG_Backend.Models.Dto
{
    public class WebSocketMessageDto
    {
        public string Type { get; set; }
        public object Payload { get; set; }
    }
}
