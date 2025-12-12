using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class GetMyServicesQuery : IRequest<OperationResult<List<Service>>>
{
    public RecordStatus? RecordStatus { get; set; }
    public long CustomerId { get; set; }
}

