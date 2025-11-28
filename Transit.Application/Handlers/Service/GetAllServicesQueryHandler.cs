using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;
internal class GetAllServicesQueryHandler : IRequestHandler<GetAllServicesQuery, OperationResult<List<Service>>>
{
    private readonly ApplicationDbContext _context;
    public GetAllServicesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<Service>>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<Service>>();
        try
        {
            var services = await _context.Services.OrderByDescending(o => o.StartDate).ToListAsync();

            if (request.RecordStatus == RecordStatus.Active)
                services = services.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active).ToList();
            else if (request.RecordStatus == RecordStatus.InActive)
                services = services.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive).ToList();
            else if (request.RecordStatus == RecordStatus.Deleted)
                services = services.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Deleted).ToList();

            if (services.Count == 0)
            {
                result.AddError(ErrorCode.Ok, "No Service Data!");
                return result;
            }
            result.Payload = services;
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }
        return result;
    }
}