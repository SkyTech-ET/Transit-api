using Transit.Domain.Models.Shared;

namespace Transit.Application;
internal class GetAllRolePrivilegesQueryHandler : IRequestHandler<GetAllRolePrivilegesQuery, OperationResult<List<RolePrivilege>>>
{
    private readonly ApplicationDbContext _context;
    public GetAllRolePrivilegesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<RolePrivilege>>> Handle(GetAllRolePrivilegesQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<RolePrivilege>>();
        try
        {
            var roles = await _context.RolePrivilege.OrderByDescending(o => o.StartDate).Where(u => u.RecordStatus != Domain.Models.Shared.RecordStatus.Deleted).ToListAsync();

            if (request.RecordStatus == RecordStatus.Active)
                roles = roles.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active).ToList();
            else if (request.RecordStatus == RecordStatus.InActive)
                roles = roles.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive).ToList();

            if (roles.Count == 0)
            {
                result.AddError(ErrorCode.Ok, "No Roles Data!");
                return result;
            }
            result.Payload = roles;
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }
        return result;
    }
}
