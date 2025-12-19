using MediatR;
using Microsoft.AspNetCore.Http;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Documents.Commands;

public class UploadServiceDocumentCommand : IRequest<OperationResult<ServiceDocument>>
{
    public long ServiceId { get; set; }
    public long UploadedByUserId { get; set; }
    public IFormFile File { get; set; } = null!;
    public DocumentType DocumentType { get; set; }
    public long? ServiceStageId { get; set; }
    public string? Description { get; set; }
    public string? FilePath { get; set; }
}
