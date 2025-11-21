namespace Transit.Api.Contracts.MOT.Request;

public class ComplianceFeedbackRequest
{
    public long ServiceId { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

