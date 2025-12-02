namespace Transit.Api.Contracts.MOT.Request;

public class AssignServiceRequest
{
    public long ServiceId { get; set; }
    public long AssignedCaseExecutorId { get; set; }
    public long AssignedAssessorId { get; set; }
    public string? AssignmentNotes { get; set; }
}

