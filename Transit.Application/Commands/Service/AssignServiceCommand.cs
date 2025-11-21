using Transit.Domain.Models.MOT;

namespace Transit.Application;

public class AssignServiceCommand : IRequest<OperationResult<Service>>
{
    public long ServiceId { get; set; }
    public long AssignedCaseExecutorId { get; set; }
    public long? AssignedAssessorId { get; set; }
    public string? AssignmentNotes { get; set; }
    public long AssignedByUserId { get; set; }
}




