using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transit.Api.Contracts.MOT.Request;
using Transit.Api.Contracts.MOT.Response;
using Transit.API.DTO.MasterData;
using Transit.API.Helpers;
using Transit.Application;
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
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] ServiceStatus? status = null,
        [FromQuery] ServiceType? serviceType = null,
        [FromQuery] RiskLevel? riskLevel = null,
        [FromQuery] long? customerId = null,
        [FromQuery] long? caseExecutorId = null,
        [FromQuery] long? assessorId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var query = new GetAllServicesQuery
        {
            Status = status,
            ServiceType = serviceType,
            RiskLevel = riskLevel,
            CustomerId = customerId,
            CaseExecutorId = caseExecutorId,
            AssessorId = assessorId,
            Search = search,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(long id)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        var query = new GetServiceByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
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
    [HttpPut("Assign")]
    public async Task<IActionResult> Assign([FromBody] AssignServiceRequest request)
    {
        var currentUserId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
        if (currentUserId == null)
            return Unauthorized("User not authenticated");

        // Verify the assigned user exists and has the correct role
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
            return NotFound("User not found");

        long? caseExecutorId = null;
        long? assessorId = null;

        if (request.Role == "caseExecutor")
        {
            var isCaseExecutor = user.UserRoles.Any(ur => ur.Role.Name == "CaseExecutor");
            if (!isCaseExecutor)
                return BadRequest("User is not a case executor");

            caseExecutorId = request.UserId;
        }
        else if (request.Role == "assessor")
        {
            var isAssessor = user.UserRoles.Any(ur => ur.Role.Name == "Assessor");
            if (!isAssessor)
                return BadRequest("User is not an assessor");

            assessorId = request.UserId;
        }
        else
        {
            return BadRequest("Invalid role. Must be 'caseExecutor' or 'assessor'");
        }

        var command = new AssignServiceCommand
        {
            ServiceId = request.ServiceId,
            AssignedCaseExecutorId = caseExecutorId ?? 0,
            AssignedAssessorId = assessorId,
            AssignedByUserId = currentUserId.Value
        };

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
