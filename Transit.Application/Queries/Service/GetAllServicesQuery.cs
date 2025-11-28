using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class GetAllServicesQuery : IRequest<OperationResult<List<Service>>>
{

        public RecordStatus? RecordStatus { get; set; }
    }
