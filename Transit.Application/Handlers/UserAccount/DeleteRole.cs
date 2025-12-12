using Transit.Domain.Data;

namespace Transit.Application;
public record DeleteRole(long id) : IRequest<OperationResult<Unit>>;

internal class DeleteRoleHandler : IRequestHandler<DeleteRole, OperationResult<Unit>>
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession _session => _httpContextAccessor.HttpContext.Session;
    public DeleteRoleHandler(ApplicationDbContext context, PasswordService password, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordService = password;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResult<Unit>> Handle(DeleteRole request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Unit>();

        var existingRole = await _context.Role.FirstOrDefaultAsync(x => x.Id == request.id);
        if (existingRole is null)
        {
            result.AddError(ErrorCode.NotFound, "Role Not found.");
            return result;
        }

        existingRole.UpdateStatus(Domain.Models.Shared.RecordStatus.Deleted);
        _context.Role.Update(existingRole);
        await _context.SaveChangesAsync();
        result.Message = "Operation success";

        return result;
    }
}
