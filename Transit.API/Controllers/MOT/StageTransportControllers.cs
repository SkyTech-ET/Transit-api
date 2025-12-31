using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.Entity;
using Transit.Api;
using Transit.Api.Contracts.User.Request;
using Transit.Api.Contracts.User.Response;
using Transit.API.DTO.MOT;
using Transit.API.Helpers;
using Transit.Application.Commands;
using Transit.Application.Documents.Commands;
using Transit.Application.Queries;
using Transit.Application.Services;
using Transit.Controllers;
using Transit.Domain.Data;
using Transit.Domain.Models.Shared;

namespace Transit.API.Controllers.MOT;

[ApiController]
[Route("api/v1/[controller]")]
public class StageTransportController : BaseController
{
    private readonly IMediator _mediator;
    private readonly string _logoPath;
    private readonly string _documentsPath;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenHandlerService _tokenHandlerService;
    public StageTransportController(IOptions<Settings> storageSettings, IMediator mediator, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, TokenHandlerService tokenHandlerService)
    {
        _mediator = mediator;
        _logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licence_Document");
        _documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "User_Documents");

        // Ensure the directories exist
        Directory.CreateDirectory(_logoPath);
        Directory.CreateDirectory(_documentsPath);
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }




    [HttpPost("Create")]
    public async Task<IActionResult> Create( [FromForm] CreateStageTransportRequest request)
    {
        // Validate the uploaded logo file
        if (request.LicenceDocumentImage == null || request.LicenceDocumentImage.Length == 0)
        {
            return BadRequest(new
            {
                Error = true,
                Message = "Licence file is required."
            });
        }
        // Set the path for saving the logo
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licence_Document");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        // Generate a unique name for the logo file
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.LicenceDocumentImage.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save the uploaded file to the server
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await request.LicenceDocumentImage.CopyToAsync(fileStream);
        }
        // Set the LicenceDocument in the request object
        request.LicenceDocument = Path.Combine("Licence_Document", uniqueFileName);

        var command = request.Adapt<CreateStageTransportCommand>();
        var result = await _mediator.Send(command);
        var userDetail = result.Payload.Adapt<StageTransportResponse>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromForm] UpdateStageTransportRequest clientRequest)
    {

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licence_Document");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var existingUser = await _context.StageTransports
            .Where(x => x.Id == clientRequest.Id)
            .FirstOrDefaultAsync();

        if (existingUser == null)
        {
            return NotFound("User not found.");
        }

        string profilePhoto = existingUser.LicenceDocument;

        // Update the logo if a new one is uploaded
        if (clientRequest.LicenceDocumentImage != null)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(clientRequest.LicenceDocumentImage.FileName);
            profilePhoto = Path.Combine("Licence_Document", uniqueFileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await clientRequest.LicenceDocumentImage.CopyToAsync(fileStream);
            }
            // Update the logo path in the request object
            profilePhoto = profilePhoto;
        }
        clientRequest.LicenceDocument = profilePhoto;


        var command = clientRequest.Adapt<UpdateStageTransportCommand>();
        var result = await _mediator.Send(command);
        var userDetail = result.Payload.Adapt<StageTransportResponse>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }


    [HttpGet("GetAllStageTransports")]
    public async Task<IActionResult> GetAllStageTransports([FromQuery] RecordStatus recordStatus)
    {
        var query = new GetAllStageTransportQuery { RecordStatus = recordStatus };
        var result = await _mediator.Send(query);

        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(result.Payload);
    }
    [HttpGet("GetAllStageTransportsByServiceStageId")]
    public async Task<IActionResult> GetAllStageTransportsByServiceStageId([FromQuery] long ServiceStageId)
    {
        var query = new GetAllStageTransportByServiceStageIdQuery { ServiceStageId= ServiceStageId };
        var result = await _mediator.Send(query);

        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(result.Payload);
    }
    [HttpGet("GetStageTransportsById")]
    public async Task<IActionResult> GetStageTransportsById([FromQuery] long Id)
    {
        var query = new GetStageTransportByIdQuery { Id = Id };
        var result = await _mediator.Send(query);

        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(result.Payload);
    }

    [HttpDelete("DeleteDocument")]
    public async Task<IActionResult> DeleteDocument([FromQuery] long Id)
    {
        var command = new DeleteStageTransportQuery { Id = Id };
        var result = await _mediator.Send(command);

        return result.IsError
            ? HandleErrorResponse(result.Errors)
            : HandleSuccessResponse(new { Message = "Stage Transport deleted successfully" });
    }

}
