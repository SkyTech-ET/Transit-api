using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Queries;

public class GetCaseExecutorAssignedServicesByIdQuery : IRequest<OperationResult<Service>>
{
    public long AssignedCaseExecutorId { get; set; }
    public long Id { get; set; }

}
