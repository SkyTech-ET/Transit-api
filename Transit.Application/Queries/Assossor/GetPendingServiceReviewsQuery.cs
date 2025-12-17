using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Queries;

public class GetPendingServiceReviewsQuery : IRequest<OperationResult<Service>>
{
    public long ServiceId { get; set; }
}