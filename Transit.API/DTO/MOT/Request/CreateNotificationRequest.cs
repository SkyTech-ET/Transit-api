using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.MOT.Request;

public class CreateNotificationRequest
{
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public long? ServiceId { get; set; }
}


