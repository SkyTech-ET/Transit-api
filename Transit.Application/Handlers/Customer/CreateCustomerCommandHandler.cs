using Transit.Domain;
using Transit.Domain.Models.MOT;
using System.Text.Json;

namespace Transit.Application;

internal class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, OperationResult<User>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession? _session => _httpContextAccessor.HttpContext?.Session;
    private readonly TokenHandlerService _tokenHandlerService;

    public CreateCustomerCommandHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }

    public async Task<OperationResult<User>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<User>();
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

        // Verify user exists
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            result.AddError(ErrorCode.NotFound, "User not found.");
            return result;
        }

        // Check if customer already exists for this user
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
        if (existingCustomer is not null)
        {
            result.AddError(ErrorCode.RecordFound, "Customer profile already exists for this user.");
            return result;
        }

        // Create customer entity
        var customer = Customer.Create(
            request.BusinessName,
            request.TINNumber,
            request.BusinessLicense,
            request.BusinessAddress,
            request.City,
            request.State,
            request.PostalCode,
            request.ContactPerson,
            request.ContactPhone,
            request.ContactEmail,
            request.BusinessType,
            request.ImportLicense,
            request.ImportLicenseExpiry,
            request.UserId,
            request.CreatedByDataEncoderId
        );

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload user with customer relationship
        var updatedUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (updatedUser == null)
        {
            result.AddError(ErrorCode.NotFound, "User not found after customer creation.");
            return result;
        }

        result.Payload = updatedUser;
        result.Message = "Operation success";

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            WriteIndented = true
        };

        //await _actionLogService.LogActionAsync(
        //    userId,
        //    JsonSerializer.Serialize(request, options),
        //    JsonSerializer.Serialize(result, options),
        //    "CreateCustomer"
        //);

        return result;
    }

    // Helper method to get the currently logged-in UserName
    private string? GetCurrentUserName()
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
