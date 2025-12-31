using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class GetAllStageTransportByServiceStageIdQuery : IRequest<OperationResult<List<StageTransport>>>
{
    public long ServiceStageId { get; set; }
}

