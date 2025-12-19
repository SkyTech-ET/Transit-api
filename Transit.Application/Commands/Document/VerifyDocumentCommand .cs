using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Commands;
public class VerifyDocumentCommand : IRequest<OperationResult<ServiceDocument>>
{
    public long DocumentId { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationNotes { get; set; }
    public long VerifiedByUserId { get; set; }
}