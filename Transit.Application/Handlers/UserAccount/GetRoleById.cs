namespace Transit.Application;
public record GetRoleById(long id) : IRequest<OperationResult<Role>>;
internal class GetRoleByIdHandler : IRequestHandler<GetRoleById, OperationResult<Role>>
{
    private readonly ApplicationDbContext _context;
    public GetRoleByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<Role>> Handle(GetRoleById request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Role>();

        var existingRole = await _context.Role.Where(x => x.Id == request.id)
            .Include(i => i.RolePrivileges)
            .ThenInclude(i => i.Privilege)
            .FirstOrDefaultAsync();

        if (existingRole is null)
        {
            result.AddError(ErrorCode.Ok, "Role Not exist.");
            return result;
        }
        result.Payload = existingRole;
        result.Message = "Operation success";

        return result;
    }
}