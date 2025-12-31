using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class CreateStageTransportCommand : IRequest<OperationResult<StageTransport>>
{
    public string FullName { get;  set; }
    public string LicenceDocument { get;  set; } 
    public string PlateNumber { get;  set; }
    public string PhoneNumber { get;  set; }
    public long? ServiceStageId { get; set; }
    public ProductAmount ProductAmount { get;  set; }
}
