using Microsoft.AspNetCore.Mvc;
using Transit.API.Services;
using Transit.Controllers;
using Transit.Domain.Models.Shared;
using Transit.Domain.Data;
using Transit.API.Helpers;
using Transit.Api.Contracts.MOT.Request;

namespace Transit.API.Controllers.MOT;

//[ApiController]
//[Route("api/v1/[controller]")]
//public class DocumentController : BaseController
//{
//    private readonly IDocumentService _documentService;
//    private readonly IFileStorageService _fileStorageService;
//    private readonly ApplicationDbContext _context;
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public DocumentController(
//        IDocumentService documentService, 
//        IFileStorageService fileStorageService,
//        ApplicationDbContext context,
//        IHttpContextAccessor httpContextAccessor)
//    {
//        _documentService = documentService;
//        _fileStorageService = fileStorageService;
//        _context = context;
//        _httpContextAccessor = httpContextAccessor;
//    }

//    [HttpPost("UploadServiceDocument")]
//    public async Task<IActionResult> UploadServiceDocument(
//        [FromForm] long serviceId,
//        [FromForm] IFormFile file,
//        [FromForm] DocumentType documentType,
//        [FromForm] long? serviceStageId = null,
//        [FromForm] string? description = null)
//    {
//        try
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("No file uploaded");

//            // Get current user ID
//            var uploadedByUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
//            if (uploadedByUserId == null)
//                return Unauthorized("User not authenticated");

//            var document = await _documentService.UploadServiceDocumentAsync(
//                serviceId,
//                uploadedByUserId.Value,
//                file,
//                documentType,
//                serviceStageId,
//                description
//            );

//            return HandleSuccessResponse(new { Message = "Document uploaded successfully", DocumentId = document.Id });
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpPost("UploadStageDocument")]
//    public async Task<IActionResult> UploadStageDocument(
//        [FromForm] long serviceStageId,
//        [FromForm] IFormFile file,
//        [FromForm] DocumentType documentType,
//        [FromForm] string? description = null)
//    {
//        try
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("No file uploaded");

//            var uploadedByUserId = GetCurrentUserId();
//            if (uploadedByUserId == null)
//                return Unauthorized("User not authenticated");

//            var document = await _documentService.UploadStageDocumentAsync(
//                serviceStageId,
//                uploadedByUserId.Value,
//                file,
//                documentType,
//                description
//            );

//            return HandleSuccessResponse(new { Message = "Document uploaded successfully", DocumentId = document.Id });
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpPost("UploadCustomerDocument")]
//    public async Task<IActionResult> UploadCustomerDocument(
//        [FromForm] long customerId,
//        [FromForm] IFormFile file,
//        [FromForm] DocumentType documentType,
//        [FromForm] string? description = null)
//    {
//        try
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("No file uploaded");

//            var uploadedByUserId = GetCurrentUserId();
//            if (uploadedByUserId == null)
//                return Unauthorized("User not authenticated");

//            var document = await _documentService.UploadCustomerDocumentAsync(
//                customerId,
//                uploadedByUserId.Value,
//                file,
//                documentType,
//                description
//            );

//            return HandleSuccessResponse(new { Message = "Document uploaded successfully", DocumentId = document.Id });
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpGet("GetServiceDocuments")]
//    public async Task<IActionResult> GetServiceDocuments([FromQuery] long serviceId)
//    {
//        try
//        {
//            var documents = await _documentService.GetServiceDocumentsAsync(serviceId);
//            return HandleSuccessResponse(documents);
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpGet("GetStageDocuments")]
//    public async Task<IActionResult> GetStageDocuments([FromQuery] long serviceStageId)
//    {
//        try
//        {
//            var documents = await _documentService.GetStageDocumentsAsync(serviceStageId);
//            return HandleSuccessResponse(documents);
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpGet("GetCustomerDocuments")]
//    public async Task<IActionResult> GetCustomerDocuments([FromQuery] long customerId)
//    {
//        try
//        {
//            var documents = await _documentService.GetCustomerDocumentsAsync(customerId);
//            return HandleSuccessResponse(documents);
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpGet("GetServiceDocumentById")]
//    public async Task<IActionResult> GetServiceDocumentById([FromQuery] long documentId)
//    {
//        try
//        {
//            var document = await _documentService.GetServiceDocumentAsync(documentId);
//            if (document == null)
//                return NotFound("Document not found");

//            return HandleSuccessResponse(document);
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpGet("GetStageDocumentById")]
//    public async Task<IActionResult> GetStageDocumentById([FromQuery] long documentId)
//    {
//        try
//        {
//            var document = await _documentService.GetStageDocumentAsync(documentId);
//            if (document == null)
//                return NotFound("Document not found");

//            return HandleSuccessResponse(document);
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpGet("GetCustomerDocumentById")]
//    public async Task<IActionResult> GetCustomerDocumentById([FromQuery] long documentId)
//    {
//        try
//        {
//            var document = await _documentService.GetCustomerDocumentAsync(documentId);
//            if (document == null)
//                return NotFound("Document not found");

//            return HandleSuccessResponse(document);
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpDelete("DeleteServiceDocument")]
//    public async Task<IActionResult> DeleteServiceDocument([FromQuery] long documentId)
//    {
//        try
//        {
//            var result = await _documentService.DeleteServiceDocumentAsync(documentId);
//            if (!result)
//                return NotFound("Document not found");

//            return HandleSuccessResponse(new { Message = "Document deleted successfully" });
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpDelete("DeleteStageDocument")]
//    public async Task<IActionResult> DeleteStageDocument([FromQuery] long documentId)
//    {
//        try
//        {
//            var result = await _documentService.DeleteStageDocumentAsync(documentId);
//            if (!result)
//                return NotFound("Document not found");

//            return HandleSuccessResponse(new { Message = "Document deleted successfully" });
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpDelete("DeleteCustomerDocument")]
//    public async Task<IActionResult> DeleteCustomerDocument([FromQuery] long documentId)
//    {
//        try
//        {
//            var result = await _documentService.DeleteCustomerDocumentAsync(documentId);
//            if (!result)
//                return NotFound("Document not found");

//            return HandleSuccessResponse(new { Message = "Document deleted successfully" });
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpPost("VerifyDocument")]
//    public async Task<IActionResult> VerifyDocument([FromBody] Transit.Api.Contracts.MOT.Request.VerifyDocumentRequest request)
//    {
//        try
//        {
//            var verifiedByUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
//            if (verifiedByUserId == null)
//                return Unauthorized("User not authenticated");

//            var result = await _documentService.VerifyDocumentAsync(request.DocumentId, verifiedByUserId.Value, request.IsVerified, request.VerificationNotes);
//            if (!result)
//                return NotFound("Document not found");

//            return HandleSuccessResponse(new { Message = "Document verification updated successfully" });
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    [HttpGet("DownloadDocument")]
//    public async Task<IActionResult> DownloadDocument([FromQuery] long documentId, [FromQuery] DocumentCategory category)
//    {
//        try
//        {
//            var filePath = await _documentService.GetDocumentPathAsync(documentId, category);
//            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
//                return NotFound("Document not found");

//            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
//            var fileName = Path.GetFileName(filePath);

//            return base.File(fileBytes, "application/octet-stream", fileName);
//        }
//        catch (Exception ex)
//        {
//            return HandleErrorResponse(ex);
//        }
//    }

//    private long? GetCurrentUserId()
//    {
//        return JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
//    }
//}
