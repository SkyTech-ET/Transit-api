using Transit.Domain.Models.Shared;
using System.Text.Json;

namespace Transit.Application;
internal class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, OperationResult<List<User>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    // private readonly ActionLogService _actionLogService;
    private readonly TokenHandlerService _tokenHandlerService;

    public GetAllUsersQueryHandler(
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
    public async Task<OperationResult<List<User>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<User>>();
        var userName = GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
        {
            userName = "";
        }
        long userId = 0;
        if (userName.Length > 0)
        {
            var existingUser = await _context.Users
                      .FirstOrDefaultAsync(x => x.Username == userName);
            userId = existingUser.Id;
        }
        try
        {
            var users = await _context.Users
     .Include(u => u.UserRoles)          // Include UserRoles
         .ThenInclude(ur => ur.Role)     // Include Role details
                                         //  .Include(u => u.UserStores)         // Include UserStores
     .OrderByDescending(o => o.StartDate)
     .ToListAsync();

            if (request.RecordStatus == RecordStatus.Active)
                users = users.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active).ToList();
            else if (request.RecordStatus == RecordStatus.InActive)
                users = users.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive).ToList();
            else if (request.RecordStatus == RecordStatus.Deleted)
                users = users.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Deleted).ToList();
            if (users.Count == 0)
            {
                result.AddError(ErrorCode.Ok, "No User Data!");
                return result;
            }
            result.Payload = users;
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                WriteIndented = true
            };

            //await _actionLogService.LogActionAsync(
            //    userId,
            //    JsonSerializer.Serialize(request, options),
            //    JsonSerializer.Serialize(result, options),
            //    "GetAllUsers"
            //);
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }
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
