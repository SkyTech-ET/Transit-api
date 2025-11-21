namespace Transit.Api.Contracts.MOT.Request;

public class AssignServiceRequest
{
    public long ServiceId { get; set; }
    public long UserId { get; set; }
    public string Role { get; set; } = string.Empty; // "caseExecutor" or "assessor"
}

