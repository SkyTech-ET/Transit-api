using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Queries;

public class DownloadDocumentQuery : IRequest<OperationResult<ServiceDocument>>
{
    public long DocumentId { get; set; }
}
