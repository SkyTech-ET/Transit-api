using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;

namespace Transit.Application.Handlers;

internal class DownloadMultipleStageDocumentsQueryHandler
    : IRequestHandler<DownloadMultipleStageDocumentsQuery, OperationResult<byte[]>>
{
    private readonly ApplicationDbContext _context;

    public DownloadMultipleStageDocumentsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<byte[]>> Handle(
        DownloadMultipleStageDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<byte[]>();

        var documents = await _context.StageDocuments
            .AsNoTracking()
            .Where(d => request.DocumentIds.Contains(d.Id))
            .ToListAsync(cancellationToken);

        if (!documents.Any())
        {
            result.AddError(ErrorCode.NotFound, "No documents found");
            return result;
        }

        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var doc in documents)
            {
                if (!File.Exists(doc.FilePath)) continue;

                var entry = zip.CreateEntry(doc.FileName);
                using var entryStream = entry.Open();
                var fileBytes = await File.ReadAllBytesAsync(doc.FilePath, cancellationToken);
                await entryStream.WriteAsync(fileBytes, cancellationToken);
            }
        }

        ms.Position = 0; // rewind the stream

        result.Payload = ms.ToArray(); // return as byte array
        result.Message = "Documents zipped successfully";

        return result;
    }
}
