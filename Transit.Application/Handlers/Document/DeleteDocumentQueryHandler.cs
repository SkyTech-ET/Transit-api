using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Handlers;

internal class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentQuery, OperationResult<bool>>
{
    private readonly ApplicationDbContext _context;

    public DeleteDocumentCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<bool>> Handle(DeleteDocumentQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        // Find the document
        var document = await _context.ServiceDocuments
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (document == null)
        {
            result.AddError(ErrorCode.NotFound, "Document not found.");
            return result;
        }

        // Soft delete by updating RecordStatus
        document.RecordStatus = RecordStatus.Deleted;
        _context.ServiceDocuments.Update(document);
        await _context.SaveChangesAsync(cancellationToken);

        result.Payload = true;
        result.Message = "Document marked as deleted successfully";
        return result;
    }
}
