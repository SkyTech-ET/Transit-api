using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Handlers;
internal class GetServiceDocumentsQueryHandler: IRequestHandler<GetServiceDocumentsQuery, OperationResult<List<ServiceDocument>>>
{
    private readonly ApplicationDbContext _context;
    public GetServiceDocumentsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<List<ServiceDocument>>> Handle( GetServiceDocumentsQuery request,   CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<ServiceDocument>>();
        try
        {
            //var services = await _context.Services.OrderByDescending(o => o.StartDate).ToListAsync();
            var servicesQuery = _context.ServiceDocuments
                                      .Where(s => s.ServiceId == request.ServiceId)
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
                result.AddError(ErrorCode.Ok, "No Document Data for this service!");
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
