using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Handlers;
internal class DownloadStageDocumentQueryHandler
    : IRequestHandler<DownloadStageDocumentQuery, OperationResult<StageDocument>>
{
    private readonly ApplicationDbContext _context;

    public DownloadStageDocumentQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<StageDocument>> Handle(
        DownloadStageDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<StageDocument>();

        // Fetch document from DB
        var document = await _context.StageDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (document == null)
        {
            result.AddError(ErrorCode.NotFound, "Document not found in database");
            return result;
        }

        // Verify file exists on disk
        if (string.IsNullOrEmpty(document.FilePath) || !File.Exists(document.FilePath))
        {
            result.AddError(ErrorCode.NotFound, "File not found on server");
            return result;
        }

        result.Payload = document;
        result.Message = "Document ready for download";

        return result;
    }
}