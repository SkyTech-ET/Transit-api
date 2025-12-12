using Transit.Domain;
using Transit.Domain.Models.MOT;

namespace Transit.Application;

internal class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;

    public GetServiceByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<Service>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();

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
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            result.AddError(ErrorCode.NotFound, "Service not found.");
            return result;
        }

        result.Payload = service;
        result.Message = "Operation success";
        return result;
    }
}


