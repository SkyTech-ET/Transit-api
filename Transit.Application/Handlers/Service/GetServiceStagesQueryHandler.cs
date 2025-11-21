using Transit.Domain;
using Transit.Domain.Models.MOT;

namespace Transit.Application;

internal class GetServiceStagesQueryHandler : IRequestHandler<GetServiceStagesQuery, OperationResult<List<ServiceStageExecution>>>
{
    private readonly ApplicationDbContext _context;

    public GetServiceStagesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<List<ServiceStageExecution>>> Handle(GetServiceStagesQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<ServiceStageExecution>>();

        var stages = await _context.ServiceStages
            .Include(s => s.StageComments)
            .Include(s => s.Documents)
            .Where(s => s.ServiceId == request.ServiceId)
            .OrderBy(s => s.Stage)
            .ToListAsync(cancellationToken);

        result.Payload = stages;
        result.Message = "Operation success";
        return result;
    }
}


