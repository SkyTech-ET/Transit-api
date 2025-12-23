using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Handlers;
internal class GetPendingServiceReviewsHandler : IRequestHandler<GetPendingServiceReviewsQuery, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;

    public GetPendingServiceReviewsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<Service>> Handle(GetPendingServiceReviewsQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();
        //var services = await _context.Services.FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        //if (services == null)
        //{
        //    result.AddError(ErrorCode.NotFound, "Service not found.");
        //    return result;
        //}

        var service = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.StageComments)
            .Include(s => s.Stages)
                .ThenInclude(stage => stage.Documents)
            .Include(s => s.Documents)
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s =>s.Status==ServiceStatus.Approved, cancellationToken);

        result.Payload = service;
        result.Message = "Operation success";
        return result;
    }
}
