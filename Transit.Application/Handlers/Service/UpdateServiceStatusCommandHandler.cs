using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

internal class UpdateServiceStatusCommandHandler : IRequestHandler<UpdateServiceStatusCommand, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;

    public UpdateServiceStatusCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<Service>> Handle(UpdateServiceStatusCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            result.AddError(ErrorCode.NotFound, "Service not found.");
            return result;
        }

        service.UpdateStatus(request.Status);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload service with relationships
        var updatedService = await _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (updatedService == null)
        {
            result.AddError(ErrorCode.NotFound, "Service not found after update.");
            return result;
        }

        result.Payload = updatedService;
        result.Message = "Operation success";
        return result;
    }
}


