using Transit.Domain;
using System.Text.Json;
namespace Transit.Application;
internal class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, OperationResult<User>>
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession _session => _httpContextAccessor.HttpContext.Session;
    // private readonly ActionLogService _actionLogService;
    private readonly TokenHandlerService _tokenHandlerService;
    public CreateUserCommandHandler(ApplicationDbContext context, PasswordService password, IHttpContextAccessor httpContextAccessor,

        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _passwordService = password;
        _httpContextAccessor = httpContextAccessor;
        // _actionLogService = actionLogService;
        _tokenHandlerService = tokenHandlerService;
    }
    public async Task<OperationResult<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
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

        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username || x.Email == request.Email);
        if (existingUser is not null)
        {
            result.AddError(ErrorCode.UserAlreadyExists, "User already exist.");
            return result;
        }
        var user = User.CreateUser(request.Username, request.Email, request.FirstName, request.LastName, request.ProfilePhoto, request.Phone, request.Password, request.IsSuperAdmin, Domain.Models.Shared.AccountStatus.Approved);
        user.UpdatePassword(_passwordService.HashPassword(request.Password));


        request.Roles.ForEach(roleid =>
        {
            user.AddRole(new UserRole
            {
                RoleId = roleid,
                UserId = user.Id
            });

        });
        _context.Users.Add(user);
        _context.SaveChanges();
        result.Payload = user;
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
        //    "CreateUser"
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
