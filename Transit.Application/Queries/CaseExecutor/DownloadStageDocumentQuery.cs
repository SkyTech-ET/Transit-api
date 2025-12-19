using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Queries;

public class DownloadStageDocumentQuery : IRequest<OperationResult<StageDocument>>
{
    public long DocumentId { get; set; }
}
