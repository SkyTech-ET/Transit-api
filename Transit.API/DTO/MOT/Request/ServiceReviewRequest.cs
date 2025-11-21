namespace Transit.Api.Contracts.MOT.Request;

public class ServiceReviewRequest
{
    public long ServiceId { get; set; }
    public bool IsApproved { get; set; }
    public string? ReviewNotes { get; set; }
}

