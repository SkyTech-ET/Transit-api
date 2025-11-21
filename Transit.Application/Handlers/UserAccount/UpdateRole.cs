using Transit.Domain.Models.Shared;

namespace Transit.Application;

public record UpdateRole(long id, string Name, string Description, List<long> Privileges, RecordStatus? RecordStatus) : IRequest<OperationResult<Role>>;
internal class UpdateRoleHandler : IRequestHandler<UpdateRole, OperationResult<Role>>
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession _session => _httpContextAccessor.HttpContext.Session;
    public UpdateRoleHandler(ApplicationDbContext context, PasswordService password, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordService = password;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResult<Role>> Handle(UpdateRole request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Role>();

        var role = await _context.Role.FirstOrDefaultAsync(x => x.Id == request.id);
        if (role is null)
        {
            result.AddError(ErrorCode.NotFound, "Role Not exist.");
            return result;
        }
        role.Update(request.Name, request.Description);
        if (request.RecordStatus is not null)
            role.UpdateStatus((Domain.Models.Shared.RecordStatus)request.RecordStatus);
        _context.Update(role);
        var existedRolePrivileges = await _context.RolePrivilege.Where(r => r.RoleId == request.id).ToListAsync();
        _context.RemoveRange(existedRolePrivileges);
        await _context.SaveChangesAsync();

        request.Privileges.ForEach(item => { role.AddRolePrivilege(new RolePrivilege() { RoleId = role.Id, PrivilegeId = item }); });
        await _context.SaveChangesAsync();
        result.Payload = role;
        result.Message = "Operation success";

        return result;
    }

}
