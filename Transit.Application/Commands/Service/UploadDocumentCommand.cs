using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MediatR;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Commands;
public class UploadDocumentCommand : IRequest<OperationResult<StageDocument>>
{
    public long ServiceId { get; set; }

    public long StageId { get; set; }

    public IFormFile File { get; set; } = null!;

    public DocumentType DocumentType { get; set; }

    public string? Description { get; set; }

    public string? FilePath { get; set; }
}
