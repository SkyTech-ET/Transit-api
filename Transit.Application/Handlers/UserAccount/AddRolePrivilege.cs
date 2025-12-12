namespace Transit.Application;

public record AddRolePrivilege(long RoleId, List<long> Privileges) : IRequest<OperationResult<Unit>>;

internal class AddRolePrivilegeHandler : IRequestHandler<AddRolePrivilege, OperationResult<Unit>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession _session => _httpContextAccessor.HttpContext.Session;
    public AddRolePrivilegeHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResult<Unit>> Handle(AddRolePrivilege request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Unit>();
        var role = await _context.Role.FirstOrDefaultAsync(x => x.Id == request.RoleId);
        if (role is null)
        {
            result.AddError(ErrorCode.NotFound, "Role Not found.");
            return result;
        }
        // check if existing Pairs exist
        // Get list of data from role privilege

        var existedRolePrivileges = await _context.RolePrivilege.Where(r => r.RoleId == request.RoleId).ToListAsync();
        _context.RemoveRange(existedRolePrivileges);
        await _context.SaveChangesAsync();
        request.Privileges.ForEach(item => { role.AddRolePrivilege(new RolePrivilege() { RoleId = role.Id, PrivilegeId = item }); });

        await _context.SaveChangesAsync();
        result.Message = "Operation success";

        return result;
    }
}

