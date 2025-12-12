using Transit.Domain.Models.Shared;

namespace Transit.Application;
internal class GetAllPrivilegesQueryHandler : IRequestHandler<GetAllPrivilegesQuery, OperationResult<List<Privilege>>>
{
    private readonly ApplicationDbContext _context;
    public GetAllPrivilegesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<Privilege>>> Handle(GetAllPrivilegesQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<Privilege>>();
        try
        {
            var privileges = await _context.Privilege.OrderByDescending(o => o.StartDate).ToListAsync();

            if (request.RecordStatus == RecordStatus.Active)
                privileges = privileges.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active).ToList();
            else if (request.RecordStatus == RecordStatus.InActive)
                privileges = privileges.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive).ToList();
            else if (request.RecordStatus == RecordStatus.Deleted)
                privileges = privileges.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Deleted).ToList();

            if (privileges.Count == 0)
            {
                result.AddError(ErrorCode.Ok, "No Privilege Data!");
                return result;
            }
            result.Payload = privileges;
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }
        return result;
    }
}
