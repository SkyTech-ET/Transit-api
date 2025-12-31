using Transit.Domain;
using Transit.Domain.Models.MOT;

namespace Transit.Application;

internal class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, OperationResult<bool>>
{
    private readonly ApplicationDbContext _context;

    public DeleteServiceCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            result.AddError(ErrorCode.NotFound, "Service not found.");
            return result;
        }

       service.UpdateStatus(Domain.Models.Shared.RecordStatus.Deleted);
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
        result.Message = "Operation success";
        return result;
    }
}


