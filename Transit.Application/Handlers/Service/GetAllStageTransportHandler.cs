using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

internal class GetAllStageTransportHandler : IRequestHandler<GetAllStageTransportQuery, OperationResult<List<StageTransport>>>
{
    private readonly ApplicationDbContext _context;
    public GetAllStageTransportHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<StageTransport>>> Handle(GetAllStageTransportQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<StageTransport>>();
        try
        {
            var services = await _context.StageTransports.OrderByDescending(o => o.StartDate).ToListAsync();

            if (request.RecordStatus == RecordStatus.Active)
                services = services.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active).ToList();
            else if (request.RecordStatus == RecordStatus.InActive)
                services = services.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive).ToList();
            else if (request.RecordStatus == RecordStatus.Deleted)
                services = services.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Deleted).ToList();

            if (services.Count == 0)
            {
                result.AddError(ErrorCode.Ok, "No Stage Transport Data!");
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