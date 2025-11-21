namespace Transit.Application.Handlers.UserAccount;
public record GetRolesByUser(long userId) : IRequest<OperationResult<List<Role>>>;
internal class GetRolesByUserQueryHandler : IRequestHandler<GetRolesByUser, OperationResult<List<Role>>>
{
    private readonly ApplicationDbContext _context;
    public GetRolesByUserQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<Role>>> Handle(GetRolesByUser request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<Role>>();
        try
        {
            var roles = await _context.UserRole.Where(u => u.UserId == request.userId && u.RecordStatus != Domain.Models.Shared.RecordStatus.Deleted)
                .OrderByDescending(o => o.StartDate)
                .Include(i => i.Role)
                .Select(i => i.Role)
                .ToListAsync();

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
