using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application.Handlers;
internal class GetCaseExecutorAssignedServicesByIdHandler : IRequestHandler<GetCaseExecutorAssignedServicesByIdQuery, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;
    public GetCaseExecutorAssignedServicesByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<Service>> Handle(GetCaseExecutorAssignedServicesByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();
        try
        {

            var service = await _context.Services.FirstOrDefaultAsync(s => s.AssignedCaseExecutorId == request.AssignedCaseExecutorId
          && s.Id == request.Id, cancellationToken);

            if (service == null)
            {
                result.AddError(ErrorCode.Ok, "No Service!");
                return result;
            }

            result.Payload = service;
            return result;


        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }
        return result;
    }
}