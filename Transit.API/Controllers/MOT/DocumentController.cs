using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Transit.Api;
using Transit.API.DTO.MOT;
using Transit.API.Helpers;
using Transit.Application.Commands;
using Transit.Application.Documents.Commands;
using Transit.Application.Queries;
using Transit.Controllers;
using Transit.Domain.Data;
using Transit.Domain.Models.Shared;

namespace Transit.API.Controllers.MOT;

[ApiController]
[Route("api/v1/[controller]")]
public class DocumentController : BaseController
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public DocumentController(
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    [HttpPost("UploadServiceDocument")]
    public async Task<IActionResult> UploadServiceDocument(
        [FromForm] UploadServiceDocumentRequest request)
    {
        var userId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (userId == null)
            return Unauthorized("User not authenticated");

        var command = new UploadServiceDocumentCommand
        {
            ServiceId = request.ServiceId,
            File = request.File,
            DocumentType =request.DocumentType,
            ServiceStageId =request.serviceStageId,
            Description = request.Description
        };

        var result = await _mediator.Send(command);
        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(result.Payload);
    }

    [HttpGet("GetServiceDocuments")]
    public async Task<IActionResult> GetServiceDocuments([FromQuery] long serviceId)
    {
        var query = new GetServiceDocumentsQuery { ServiceId = serviceId };
        var result = await _mediator.Send(query);

        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(result.Payload);
    }

    [HttpDelete("DeleteDocument")]
    public async Task<IActionResult> DeleteDocument([FromQuery] long documentId)
    {
        var command = new DeleteDocumentQuery { DocumentId = documentId };
        var result = await _mediator.Send(command);

        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(new { Message = "Document deleted successfully" });
    }

    [HttpPost("VerifyDocument")]
    public async Task<IActionResult> VerifyDocument([FromBody] VerifyDocumentCommand command)
    {
        var userId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (userId == null)
            return Unauthorized("User not authenticated");

        command.VerifiedByUserId = userId.Value;

        var result = await _mediator.Send(command);
        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(result.Payload);
    }

    [HttpGet("DownloadDocument")]
    public async Task<IActionResult> DownloadDocument(
    [FromQuery] long documentId)
    {
        var query = new DownloadDocumentQuery
        {
            DocumentId = documentId,
        };

        var result = await _mediator.Send(query);

        var docmunet = result.Payload.Adapt<DownloadDocumentResult>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(docmunet);
    }
    [HttpPost("DownloadDocuments")]
    public async Task<IActionResult> DownloadDocuments(
        [FromBody] List<long> documentIds)
    {
        var query = new DownloadMultipleDocumentsQuery
        {
            DocumentIds = documentIds
        };

        var result = await _mediator.Send(query);

        var docmunetList= result.Payload.Adapt<List<DownloadDocumentResult>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(docmunetList);

    }
}
