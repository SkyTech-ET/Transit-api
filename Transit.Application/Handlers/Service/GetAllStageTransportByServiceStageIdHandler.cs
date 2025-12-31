using MediatR;
using Microsoft.EntityFrameworkCore;
using Transit.Application;
using Transit.Application.Model;
using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

internal class GetAllStageTransportByServiceStageIdHandler : IRequestHandler<GetAllStageTransportByServiceStageIdQuery, OperationResult<List<StageTransport>>>
{
    private readonly ApplicationDbContext _context;

    public GetAllStageTransportByServiceStageIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<List<StageTransport>>> Handle(
        GetAllStageTransportByServiceStageIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<StageTransport>>();

        try
        {
            var stageTransports = await _context.StageTransports
                .Where(s => s.ServiceStageId == request.ServiceStageId)
                .ToListAsync(cancellationToken);

            if (stageTransports.Count == 0)
            {
                result.AddError(ErrorCode.NotFound, "Stage Transport not found.");
                return result;
            }

            result.Payload = stageTransports;
            result.Message = "Operation success";
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
            return result;
        }
    }
}
