using Transit.Domain;
using Transit.Domain.Models.Shared;
using System.Text.Json;

namespace Transit.Application;

public record UpdateUser(long id, RecordStatus? RecordStatus, string FirstName, string LastName, string ProfilePhoto, string Phone, string Email, string UserName, bool IsSuperAdmin, List<long> Roles, long? OrganizationId) : IRequest<OperationResult<User>>;
internal class UpdateUserHandler : IRequestHandler<UpdateUser, OperationResult<User>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    // private readonly ActionLogService _actionLogService;
    private readonly TokenHandlerService _tokenHandlerService;

    public UpdateUserHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,

        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        // _actionLogService = actionLogService;
        _tokenHandlerService = tokenHandlerService;
    }
    public async Task<OperationResult<User>> Handle(UpdateUser request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<User>();

        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.id);

        if (existingUser is null)
        {
            result.AddError(ErrorCode.NotFound, "User Not exist.");
            return result;
        }
        // Update user information
        existingUser.UpdateUser(
            request.FirstName,
            request.LastName,
            request.ProfilePhoto,
            request.Phone,
            existingUser.IsSuperAdmin,
            request.UserName,
            request.Email,
            request.RecordStatus ?? RecordStatus.Active,
            AccountStatus.Approved// Set default RecordStatus if null
        );

        _context.Users.Update(existingUser);

        var existedRolePrivileges = await _context.UserRole.Where(r => r.UserId == request.id).ToListAsync();
        _context.RemoveRange(existedRolePrivileges);
        await _context.SaveChangesAsync();

        request.Roles.ForEach(item => { existingUser.AddRole(new UserRole() { RoleId = item, UserId = existingUser.Id }); });
        await _context.SaveChangesAsync();
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
        //    "UpdateUser"
        //);
        return result;
    }
    // Helper method to get the currently logged-in UserName

}
