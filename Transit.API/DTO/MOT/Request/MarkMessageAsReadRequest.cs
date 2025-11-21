namespace Transit.Api.Contracts.MOT.Request;

public class MarkMessageAsReadRequest
{
    public long MessageId { get; set; }
    public long UserId { get; set; }
}


