using Transit.Domain.Models.MOT;

namespace Transit.Application;

public class GetServiceStagesQuery : IRequest<OperationResult<List<ServiceStageExecution>>>
{
    public long ServiceId { get; set; }
}

