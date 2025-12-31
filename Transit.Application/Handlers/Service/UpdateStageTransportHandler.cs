using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain;

namespace Transit.Application;

internal class UpdateStageTransportHandler : IRequestHandler<UpdateStageTransportCommand, OperationResult<StageTransport>>
{
    private readonly ApplicationDbContext _context;

    public UpdateStageTransportHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<StageTransport>> Handle(UpdateStageTransportCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<StageTransport>();

        // Find existing StageTransport
        var stageTransport = await _context.StageTransports
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (stageTransport == null)
        {
            result.Message = $"StageTransport with ID {request.Id} not found.";
            return result;
        }

        // Update entity
        stageTransport.UpdateTransport(
            request.FullName,
            request.LicenceDocument,
            request.PlateNumber,
            request.PhoneNumber,
            request.ProductAmount,
            request.ServiceStageId,
            request.RecordStatus
        );

        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        result.Payload = stageTransport;
        result.Message = "StageTransport updated successfully.";
        return result;
    }
}
