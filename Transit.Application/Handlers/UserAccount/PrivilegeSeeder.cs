namespace Transit.Application;

public record PrivilegeSeeder(List<PrivilegeDto> PrivilegeDtos) : IRequest<OperationResult<Unit>>;
internal class PrivilegeSeederHandler : IRequestHandler<PrivilegeSeeder, OperationResult<Unit>>
{
    private readonly ApplicationDbContext _context;
    public PrivilegeSeederHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<Unit>> Handle(PrivilegeSeeder request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Unit>();

        foreach (var item in request.PrivilegeDtos)
        {
            var existingPrivilege = await _context.Privilege.FirstOrDefaultAsync(x => x.Action == item.Action);
            if (existingPrivilege is not null)
                continue;

            var privilege = Privilege.Create(item.Action, item.Description);
            await _context.Privilege.AddAsync(privilege);
        }

        await _context.SaveChangesAsync();
        result.Message = "Operation success";

        return result;
    }
}
