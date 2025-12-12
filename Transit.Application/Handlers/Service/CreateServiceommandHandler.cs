using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using System.Text.Json;

namespace Transit.Application;

internal class CreateServiceommandHandler : IRequestHandler<CreateServiceCommand, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession? _session => _httpContextAccessor.HttpContext?.Session;
    private readonly TokenHandlerService _tokenHandlerService;

    public CreateServiceommandHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }

    public async Task<OperationResult<Service>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();
        var userName = GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
        {
            userName = "";
        }
        long userId = 0;
        if (userName.Length > 0)
        {
            var existingUsers = await _context.Users
                      .FirstOrDefaultAsync(x => x.Username == userName);
            if (existingUsers != null)
                userId = existingUsers.Id;
        }

        // Verify customer exists and is verified
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            result.AddError(ErrorCode.NotFound, "Customer not found or not verified.");
            return result;
        }

        // Generate service number
        var serviceNumber = $"SRV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        // Create service entity
        var service = Service.Create(
            serviceNumber,
            request.ItemDescription,
            request.RouteCategory,
            request.DeclaredValue,
            request.TaxCategory,
            request.CountryOfOrigin,
            request.ServiceType,
            request.CustomerId,
            request.CreatedByUserId
        );

        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken);

        // Create initial service stages based on service type
        await CreateServiceStages(service.Id, request.ServiceType, cancellationToken);

        // Reload service with relationships
        var updatedService = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == service.Id, cancellationToken);

        if (updatedService == null)
        {
            result.AddError(ErrorCode.NotFound, "Service not found after creation.");
            return result;
        }

        result.Payload = updatedService;
        result.Message = "Operation success";

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };

        return result;
    }

    private async Task CreateServiceStages(long serviceId, ServiceType serviceType, CancellationToken cancellationToken)
    {
        var stages = new List<ServiceStageExecution>();

        // Create stages based on service type
        switch (serviceType)
        {
            case ServiceType.Multimodal:
                stages.AddRange(new[]
                {
                    ServiceStageExecution.Create(serviceId, ServiceStage.PrepaymentInvoice),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DropRisk),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DeliveryOrder),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Inspection),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Transportation),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Clearance)
                });
                break;
            case ServiceType.Unimodal:
                stages.AddRange(new[]
                {
                    ServiceStageExecution.Create(serviceId, ServiceStage.PrepaymentInvoice),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DropRisk),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DeliveryOrder),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Inspection),
                    ServiceStageExecution.Create(serviceId, ServiceStage.LocalPermission),
                    ServiceStageExecution.Create(serviceId, ServiceStage.Arrival),
                    ServiceStageExecution.Create(serviceId, ServiceStage.StoreSettlement)
                });
                break;
            default:
                stages.AddRange(new[]
                {
                    ServiceStageExecution.Create(serviceId, ServiceStage.PrepaymentInvoice),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DropRisk),
                    ServiceStageExecution.Create(serviceId, ServiceStage.DeliveryOrder)
                });
                break;
        }

        _context.ServiceStages.AddRange(stages);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private string? GetCurrentUserName()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return null;

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var claims = _tokenHandlerService.GetClaims(token);
        var userNameClaim = claims?.FirstOrDefault(c => c.Type == "userName");
        return userNameClaim?.Value;
    }
}




