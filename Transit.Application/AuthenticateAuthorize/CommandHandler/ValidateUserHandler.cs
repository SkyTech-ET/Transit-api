

namespace Transit.Application;

public class ValidateUserHandler : IRequestHandler<ValidateUserCommand, OperationResult<UserTokenValidationResponse>>
{
    private readonly TokenHandlerService _tokenHandlerService;
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;
    public ValidateUserHandler(TokenHandlerService tokenHandlerService, IMediator mediator, ApplicationDbContext context)
    {
        _tokenHandlerService = tokenHandlerService;
        _mediator = mediator;
        _context = context;
    }

    public async Task<OperationResult<UserTokenValidationResponse>> Handle(ValidateUserCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<UserTokenValidationResponse>();
        try
        {
            if (!_tokenHandlerService.ValidateToken(request.AccessToken)) //decrypt and validate access token
            {
                result.AddError(ErrorCode.UnAuthorized, "token is invalid.");
                return result;
            }

            var claims = _tokenHandlerService.GetClaims(request.AccessToken); // get claims from the token
            var userName = claims.Where(x => x.Type == "userName").FirstOrDefault()?.Value;

            if (!await AuthorizeUser(userName, request.ApiResource)) // check if user is allowed to access
            {
                result.AddError(ErrorCode.UnAuthorized, "User is not Authorized.");
                return result;
            }
            result.Payload = new UserTokenValidationResponse() { UserName = userName };
            result.Message = "Operation Success";
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
            return result;
        }
    }

    private async Task<bool> AuthorizeUser(string userName, string privilege)
    {
        if (userName is null) return false;

        var user = await _context.Users.FirstOrDefaultAsync(s => s.Username == userName);
        if (user is null) return false;


        if (user.IsSuperAdmin) return true;
        var userRole = await _context.UserRole.FirstOrDefaultAsync(s => s.UserId == user.Id);
        if (userRole is null) return false;


        var privilegeList = await _context.RolePrivilege.Where(s => s.RoleId == userRole.RoleId)
            .Include(i => i.Privilege)
            .Select(s => s.Privilege.Action).ToListAsync();
        if (!privilegeList.Contains(privilege)) return false;

        return true;
    }

}

