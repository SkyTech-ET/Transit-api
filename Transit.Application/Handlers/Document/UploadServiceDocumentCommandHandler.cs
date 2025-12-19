using Microsoft.EntityFrameworkCore;
using Transit.Application.Documents.Commands;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Handlers;

internal class UploadServiceDocumentCommandHandler
    : IRequestHandler<UploadServiceDocumentCommand, OperationResult<ServiceDocument>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenHandlerService _tokenHandlerService;

    public UploadServiceDocumentCommandHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }

    public async Task<OperationResult<ServiceDocument>> Handle(
        UploadServiceDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<ServiceDocument>();

        try
        {
            var userId = await GetCurrentUserIdAsync(cancellationToken);
            if (userId == 0)
            {
                result.AddError(ErrorCode.NotFound, "User not authenticated.");
                return result;
            }

            var serviceExists = await _context.Services
                .AnyAsync(s => s.Id == request.ServiceId, cancellationToken);

            if (!serviceExists)
            {
                result.AddError(ErrorCode.NotFound, "Service not found.");
                return result;
            }
            var uploadsFolder = Path.Combine("Uploads", "Services", DateTime.UtcNow.ToString("yyyyMMdd"));
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            var document = ServiceDocument.Create(
                fileName: uniqueFileName,
                filePath: filePath,
                originalFileName: request.File.FileName,
                fileExtension: Path.GetExtension(request.File.FileName),
                fileSizeBytes: request.File.Length,
                mimeType: request.File.ContentType,
                documentType: request.DocumentType,
                serviceId: request.ServiceId,
                uploadedByUserId: userId,
                serviceStageId: request.ServiceStageId,
                description: request.Description
            );

            _context.ServiceDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            result.Payload = document;
            result.Message = "Service document uploaded successfully.";
        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }

        return result;
    }

    private async Task<long> GetCurrentUserIdAsync(CancellationToken cancellationToken)
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?
            .Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return 0;

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var claims = _tokenHandlerService.GetClaims(token);

        var userName = claims?.FirstOrDefault(c => c.Type == "userName")?.Value;
        if (string.IsNullOrEmpty(userName))
            return 0;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == userName, cancellationToken);

        return user?.Id ?? 0;
    }
}
