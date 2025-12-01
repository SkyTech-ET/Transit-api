using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Handlers;
internal class GetCaseExecutorAssignedServicesByIdHandler : IRequestHandler<GetCaseExecutorAssignedServicesByIdQuery, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;
    public GetCaseExecutorAssignedServicesByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<Service>> Handle(GetCaseExecutorAssignedServicesByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();

        var existingService = await _context.Services
                  .Include(s => s.Customer)
                  .Include(s => s.Stages)
                      .ThenInclude(stage => stage.StageComments)
                  .Include(s => s.Stages)
                      .ThenInclude(stage => stage.Documents)
                  .Include(s => s.Documents)
                  .Include(s => s.Messages)
                  .AsNoTracking()  // <-- boosts read performance
                  .FirstOrDefaultAsync(x =>
                          x.Id == request.Id &&
                          x.AssignedCaseExecutorId == request.AssignedCaseExecutorId,
                      cancellationToken);

        if (existingService is null)
        {
            result.AddError(ErrorCode.NotFound, "Service does not exist or access denied.");
            return result;
        }

        result.Payload = existingService;
        result.Message = "Operation successful";
        return result;
    }
}