using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Handlers;
internal class GetAssignedServicesHandler : IRequestHandler<GetAssignedServicesQuery, OperationResult<List<Service>>>
{
    private readonly ApplicationDbContext _context;
    public GetAssignedServicesHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<Service>>> Handle(GetAssignedServicesQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<Service>>();
        try
        {
            //var services = await _context.Services.OrderByDescending(o => o.StartDate).ToListAsync();
            var servicesQuery = _context.Services
                                      .Where(s => s.AssignedCaseExecutorId == request.AssignedCaseExecutorId)
                                      .OrderByDescending(s => s.StartDate)
                                      .AsQueryable();

            if (request.RecordStatus == RecordStatus.Active)
                servicesQuery = servicesQuery.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active);
            else if (request.RecordStatus == RecordStatus.InActive)
                servicesQuery = servicesQuery.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive);
            else if (request.RecordStatus == RecordStatus.Deleted)
                servicesQuery = servicesQuery.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Deleted);

            var services = await servicesQuery.ToListAsync(cancellationToken);

            if (!services.Any())
            {
                result.AddError(ErrorCode.Ok, "No Service Data for this user!");
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