using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class UpdateServiceStatusCommand : IRequest<OperationResult<Service>>
{
    public long Id { get; set; }
    public ServiceStatus Status { get; set; }
}

