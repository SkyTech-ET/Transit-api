using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;

namespace Transit.Application;
public class DeleteDocumentQuery : IRequest<OperationResult<bool>>
{
    public long DocumentId { get; set; }
}