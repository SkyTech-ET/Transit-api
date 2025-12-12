using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Domain.Models.MOT;

namespace Transit.Application.Handlers;

public record GetServiceById(long id) : IRequest<OperationResult<Service>>;
internal class GetServiceByIdHandler : IRequestHandler<GetServiceById, OperationResult<Service>>
{
    private readonly ApplicationDbContext _context;
    public GetServiceByIdHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<Service>> Handle(GetServiceById request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Service>();

        var existingServices = await _context.Services.FirstOrDefaultAsync(x => x.Id == request.id);

        if (existingServices is null)
        {
            result.AddError(ErrorCode.Ok, "Service Not exist.");
            return result;
        }
        result.Payload = existingServices;
        result.Message = "Operation success";

        return result;
    }
}
