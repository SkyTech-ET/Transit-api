using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class UpdateServiceStageCommand : IRequest<OperationResult<ServiceStageExecution>>
{
    public long ServiceStageId { get; set; }
    public long ServiceId { get; set; }
    public StageStatus Status { get; set; }
    public string? Notes { get; set; }
    public long UpdatedByUserId { get; set; }
}

