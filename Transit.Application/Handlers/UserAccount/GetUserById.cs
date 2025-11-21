using System.Text.Json;

namespace Transit.Application;
public record GetUserById(long id) : IRequest<OperationResult<User>>;
internal class GetUserByIdHandler : IRequestHandler<GetUserById, OperationResult<User>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    // private readonly ActionLogService _actionLogService;
    private readonly TokenHandlerService _tokenHandlerService;

    public GetUserByIdHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        // ActionLogService actionLogService,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        // _actionLogService = actionLogService;
        _tokenHandlerService = tokenHandlerService;
    }
    public async Task<OperationResult<User>> Handle(GetUserById request, CancellationToken cancellationToken)
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
            userId = existingUsers.Id;
        }
        var existingUser = await _context.Users
            .Include(i => i.UserRoles)
                .ThenInclude(i => i.Role)
            // .Include(i => i.UserStores) // Assuming the navigation property is UserStores
            // .ThenInclude(us => us.Store) // Assuming UserStore has a Store navigation property
            .FirstOrDefaultAsync(x => x.Id == request.id);


        if (existingUser is null)
        {
            result.AddError(ErrorCode.Ok, "User Not exist.");
            return result;
        }
        result.Payload = existingUser;
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
        //    "GetUsersById"
        //);
        return result;
    }
    // Helper method to get the currently logged-in UserName
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