using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transit.Application.Queries;
public class DownloadMultipleStageDocumentsQuery : IRequest<OperationResult<byte[]>>
{
    public List<long> DocumentIds { get; set; } = new();
}
