using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Queries;
public class GetMyServicesQuery : IRequest<OperationResult<List<Service>>>
{
    public RecordStatus? RecordStatus { get; set; }
    public long UserId { get; set; }
}