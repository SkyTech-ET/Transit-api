using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.MOT.Request;

public class SendMessageRequest
{
    public long ServiceId { get; set; }
    public long? RecipientId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
}


