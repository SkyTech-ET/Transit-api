using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transit.Application;

public class DeleteStageTransportQuery : IRequest<OperationResult<bool>>
{
    public long Id { get; set; }
}
