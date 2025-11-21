using Transit.Domain.Models.Shared;

namespace Transit.Application;
internal class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, OperationResult<List<Role>>>
{
    private readonly ApplicationDbContext _context;
    public GetAllRolesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<Role>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<Role>>();
        try
        {
            var roles = await _context.Role.OrderByDescending(o => o.StartDate).ToListAsync();

            if (request.RecordStatus == RecordStatus.Active)
                roles = roles.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active).ToList();
            else if (request.RecordStatus == RecordStatus.InActive)
                roles = roles.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive).ToList();
            else if (request.RecordStatus == RecordStatus.Deleted)
                roles = roles.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Deleted).ToList();

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
