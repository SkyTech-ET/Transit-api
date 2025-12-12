using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Transit.Application.Commands;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Handlers
{
    internal class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, OperationResult<StageDocument>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private readonly TokenHandlerService _tokenHandlerService;

        public UploadDocumentHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, TokenHandlerService tokenHandlerService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _tokenHandlerService = tokenHandlerService;
        }

        public async Task<OperationResult<StageDocument>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<StageDocument>();

            try
            {
                long userId = 0;

                // Get current logged-in user ID (optional)
                var userName = GetCurrentUserName() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    var existingUser = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Username == userName, cancellationToken);

                    if (existingUser != null)
                        userId = existingUser.Id;
                }

                // Validate ServiceStage exists
                var serviceStage = await _context.ServiceStages
                    .FindAsync(new object[] { request.StageId }, cancellationToken);

                if (serviceStage == null)
                {
                    result.AddError(ErrorCode.NotFound, "Service stage not found.");
                    return result;
                }

                if (serviceStage.ServiceId != request.ServiceId)
                {
                    result.AddError(ErrorCode.ValidationError, "ServiceId does not match the stage.");
                    return result;
                }

                // Save file to disk (can be replaced with cloud storage)
                var uploadsFolder = Path.Combine("Uploads", DateTime.UtcNow.ToString("yyyyMMdd"));
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                // Create StageDocument using factory method
                var document = StageDocument.Create(
                    fileName: uniqueFileName,
                    filePath: filePath,
                    originalFileName: request.File.FileName,
                    fileExtension: Path.GetExtension(request.File.FileName),
                    fileSizeBytes: request.File.Length,
                    mimeType: request.File.ContentType,
                    documentType: request.DocumentType,
                    serviceStageId: request.StageId,
                    uploadedByUserId: userId,
                    description: request.Description
                );

                _context.StageDocuments.Add(document);
                await _context.SaveChangesAsync(cancellationToken);

                result.Payload = document;
                result.Message = "Document uploaded successfully.";
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.ServerError, ex.Message);
            }

            return result;
        }

        // Helper: get currently logged-in username from Bearer token
        private string GetCurrentUserName()
        {
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return null; // No token available in request
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            var claims = _tokenHandlerService.GetClaims(token); // Use TokenHandlerService to get claims

            var userNameClaim = claims?.FirstOrDefault(c => c.Type == "userName");
            return userNameClaim?.Value; // Return the username or null if not found
        }
    }
}
