using Transit.Domain.Models.Shared;

namespace Transit.Application;

public record UpdatePrivilege(long id, string action, string description, RecordStatus? RecordStatus) : IRequest<OperationResult<Privilege>>;
internal class UpdatePrivilegeHandler : IRequestHandler<UpdatePrivilege, OperationResult<Privilege>>
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession _session => _httpContextAccessor.HttpContext.Session;
    public UpdatePrivilegeHandler(ApplicationDbContext context, PasswordService password, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordService = password;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResult<Privilege>> Handle(UpdatePrivilege request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Privilege>();

        var existingPrivilege = await _context.Privilege.FirstOrDefaultAsync(x => x.Id == request.id);
        if (existingPrivilege is null)
        {
            result.AddError(ErrorCode.NotFound, "Privilege Not exist.");
            return result;
        }
        existingPrivilege.Update(request.action, request.description);
        if (request.RecordStatus is not null)
            existingPrivilege.UpdateStatus((Domain.Models.Shared.RecordStatus)request.RecordStatus);

        _context.Privilege.Update(existingPrivilege);
        await _context.SaveChangesAsync();
        result.Payload = existingPrivilege;
        result.Message = "Operation success";

        return result;
    }

}
