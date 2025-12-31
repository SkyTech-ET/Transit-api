using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transit.Application;

internal class DeleteStageTransportHandler : IRequestHandler<DeleteStageTransportQuery, OperationResult<bool>>
{
    private readonly ApplicationDbContext _context;

    public DeleteStageTransportHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<bool>> Handle(DeleteStageTransportQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var service = await _context.StageTransports
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            result.AddError(ErrorCode.NotFound, "Stage Transport not found.");
            return result;
        }


        service.UpdateStatus(Domain.Models.Shared.RecordStatus.Deleted);
        _context.StageTransports.Update(service);
        await _context.SaveChangesAsync();
        result.Message = "Operation success";
        return result;
    }
}