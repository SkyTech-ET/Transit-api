namespace Transit.Application;
public record GetPrivilegeById(long id) : IRequest<OperationResult<Privilege>>;
internal class GetPrivilegeByIdHandler : IRequestHandler<GetPrivilegeById, OperationResult<Privilege>>
{
    private readonly ApplicationDbContext _context;
    public GetPrivilegeByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<Privilege>> Handle(GetPrivilegeById request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Privilege>();

        var existingPrivilege = await _context.Privilege.FirstOrDefaultAsync(x => x.Id == request.id);

        if (existingPrivilege is null)
        {
            result.AddError(ErrorCode.Ok, "Privilege Not exist.");
            return result;
        }
        result.Payload = existingPrivilege;
        result.Message = "Operation success";

        return result;
    }
}