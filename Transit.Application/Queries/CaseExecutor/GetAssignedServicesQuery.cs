using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Queries;
public class GetAssignedServicesQuery : IRequest<OperationResult<List<Service>>>
{
    public long AssignedCaseExecutorId { get; set; }
    public RecordStatus? RecordStatus { get; set; }

}