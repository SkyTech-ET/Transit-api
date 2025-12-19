using Transit.Application.Queries;
using Transit.Domain.Models.MOT;
using Microsoft.EntityFrameworkCore;

namespace Transit.Application.Handlers;

internal class DownloadDocumentQueryHandler
    : IRequestHandler<DownloadDocumentQuery, OperationResult<ServiceDocument>>
{
    private readonly ApplicationDbContext _context;

    public DownloadDocumentQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<ServiceDocument>> Handle(
        DownloadDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<ServiceDocument>();

        // Fetch document from DB
        var document = await _context.ServiceDocuments
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
