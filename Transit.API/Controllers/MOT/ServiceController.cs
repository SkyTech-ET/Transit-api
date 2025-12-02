using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transit.Api;
using Transit.Api.Contracts.MOT.Request;
using Transit.Api.Contracts.MOT.Response;
using Transit.API.DTO.MasterData;
using Transit.API.Helpers;
using Transit.Application;
using Transit.Application.Handlers;
using Transit.Controllers;
using Transit.Domain.Data;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.API.Controllers.MOT;

[ApiController]
[Route("api/v1/[controller]")]
public class ServiceController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServiceController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
    {
        // Get current authenticated user
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        // Map request to command and set CreatedByUserId
        var command = request.Adapt<CreateServiceCommand>();
        command.CustomerId = request.CustomerId;
        command.CreatedByUserId = currentUserId.Value;

        // Execute command
        var result = await _mediator.Send(command);

        if (result.IsError)
            return HandleErrorResponse(result.Errors);

        // Map payload to clean response DTO to avoid circular reference
        var service = result.Payload;
        var response = new ServiceResponse
        {
            Id = service.Id,
            ServiceNumber = service.ServiceNumber,
            ItemDescription = service.ItemDescription,
            DeclaredValue = service.DeclaredValue,
            CountryOfOrigin = service.CountryOfOrigin,
            ServiceType = service.ServiceType,
            CustomerId = service.CustomerId,
            CreatedByUserId = service.CreatedByUserId,
            Stages = service.Stages?.Select(s => new ServiceStageResponse
            {
                Id = s.Id,
                Stage = s.Stage,
                Status = s.RecordStatus,
                CreatedDate = s.CreatedDate
            }).ToList() ?? new List<ServiceStageResponse>()
        };

        return Ok(new
        {
            statusCode = 200,
            error = false,
            errors = Array.Empty<string>(),
            message = "Operation Success",
            response = response
        });
    }


    /// <summary>
    /// Get all services with filtering and pagination
    /// </summary>
    [HttpGet("GetAll/{recordStatus}")]
    public async Task<IActionResult> GetAll(RecordStatus? recordStatus)
    {
        var query = new GetAllServicesQuery { RecordStatus = recordStatus };
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<List<ServiceDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(long Id)
    {
        var query = new GetServiceById(Id);
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<ServiceDetail>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }


    /// <summary>
    /// Update service
    /// </summary>
    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] UpdateServiceRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var command = new UpdateServiceCommand
        {
            Id = request.Id,
            ItemDescription = request.ItemDescription,
            RouteCategory = request.RouteCategory,
            DeclaredValue = request.DeclaredValue,
            TaxCategory = request.TaxCategory,
            CountryOfOrigin = request.CountryOfOrigin,
            RiskLevel = request.RiskLevel
        };

        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Delete service
    /// </summary>
    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var command = new DeleteServiceCommand { Id = id };
        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(new { Message = "Service deleted successfully" });
    }

    /// <summary>
    /// Update service status
    /// </summary>
    [HttpPut("UpdateStatus")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateServiceStatusRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var command = new UpdateServiceStatusCommand
        {
            Id = request.Id,
            Status = request.Status
        };

        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Assign service to case executor or assessor
    /// </summary>
    [HttpPut("AssignServices")]
    public async Task<IActionResult> AssignServices([FromBody] AssignServiceRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");
        var command = request.Adapt<AssignServiceCommand>();

        // Assign the current user ID
        command.AssignedAssessorId = currentUserId.Value;

        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Get service stages
    /// </summary>
    [HttpGet("GetStages")]
    public async Task<IActionResult> GetStages([FromQuery] long serviceId)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var query = new GetServiceStagesQuery { ServiceId = serviceId };
        var result = await _mediator.Send(query);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Update stage status
    /// </summary>
    [HttpPut("UpdateStageStatus")]
    public async Task<IActionResult> UpdateStageStatus([FromBody] UpdateStageStatusRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var command = new UpdateServiceStageCommand
        {
            ServiceStageId = request.StageId,
            Status = request.Status,
            Notes = request.Comments,
            UpdatedByUserId = currentUserId.Value
        };

        var result = await _mediator.Send(command);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }
}
