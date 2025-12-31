using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain;
using Transit.Domain.Models.MOT;

namespace Transit.Application;

public class GetStageTransportByIdQuery : IRequest<OperationResult<StageTransport>>
{
    public long Id { get; set; }
}
