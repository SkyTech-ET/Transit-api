using Transit.Domain.Data;

namespace Transit.Application;
internal class CreatePrivilegeCommandHandler : IRequestHandler<CreatePrivilegeCommand, OperationResult<Privilege>>
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession _session => _httpContextAccessor.HttpContext.Session;
    public CreatePrivilegeCommandHandler(ApplicationDbContext context, PasswordService password, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordService = password;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResult<Privilege>> Handle(CreatePrivilegeCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Privilege>();

        var existingPrivilege = await _context.Privilege.FirstOrDefaultAsync(x => x.Action == request.Action);
        if (existingPrivilege is not null)
        {
            result.AddError(ErrorCode.RecordFound, "Privilege already exist.");
            return result;
        }
        var role = Privilege.Create(request.Action, request.Description);
        await _context.Privilege.AddAsync(role);
        await _context.SaveChangesAsync();
        result.Payload = role;
        result.Message = "Operation success";

        return result;
    }
}
