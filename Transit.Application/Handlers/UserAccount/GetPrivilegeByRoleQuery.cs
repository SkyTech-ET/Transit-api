namespace Transit.Application.Handlers.UserAccount;
public record GetPrivilegeByRoleQuery(long roleId) : IRequest<OperationResult<List<Privilege>>>;
internal class GetPrivilegeByRoleQueryHandler : IRequestHandler<GetPrivilegeByRoleQuery, OperationResult<List<Privilege>>>
{
    private readonly ApplicationDbContext _context;
    public GetPrivilegeByRoleQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<Privilege>>> Handle(GetPrivilegeByRoleQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<Privilege>>();
        try
        {
            var privileges = await _context.RolePrivilege.Where(u => u.RoleId == request.roleId && u.RecordStatus != Domain.Models.Shared.RecordStatus.Deleted)
                .OrderByDescending(o => o.StartDate)
                .Include(i => i.Privilege)
                .Select(i => i.Privilege)
                .ToListAsync();

            if (privileges.Count == 0)
            {
                result.AddError(ErrorCode.Ok, "No Privilege Data!");
                return result;
            }
            result.Payload = privileges;
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }
        return result;
    }
}
