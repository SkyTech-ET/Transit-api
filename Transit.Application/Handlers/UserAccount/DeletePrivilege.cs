namespace Transit.Application;
public record DeletePrivilege(long id) : IRequest<OperationResult<Unit>>;

internal class DeletePrivilegeHandler : IRequestHandler<DeletePrivilege, OperationResult<Unit>>
{
    private readonly ApplicationDbContext _context;
    public DeletePrivilegeHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<Unit>> Handle(DeletePrivilege request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Unit>();

        var existingPrivilege = await _context.Privilege.FirstOrDefaultAsync(x => x.Id == request.id);
        if (existingPrivilege is null)
        {
            result.AddError(ErrorCode.NotFound, "Privilege Not found.");
            return result;
        }

        existingPrivilege.UpdateStatus(Domain.Models.Shared.RecordStatus.Deleted);
        _context.Privilege.Update(existingPrivilege);
        await _context.SaveChangesAsync();
        result.Message = "Operation success";

        return result;
    }
}
