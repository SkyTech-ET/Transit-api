using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transit.Application.Queries;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using Transit.Domain.Models.Shared;

namespace Transit.Application;
internal class GetAllCustomer : IRequestHandler<GetAllCustomersQuery, OperationResult<List<Customer>>>
{
    private readonly ApplicationDbContext _context;
    public GetAllCustomer(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<OperationResult<List<Customer>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<List<Customer>>();
        try
        {
            var customers = await _context.Customers.OrderByDescending(o => o.StartDate).ToListAsync();

            if (request.RecordStatus == RecordStatus.Active)
                customers = customers.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Active).ToList();
            else if (request.RecordStatus == RecordStatus.InActive)
                customers = customers.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.InActive).ToList();
            else if (request.RecordStatus == RecordStatus.Deleted)
                customers = customers.Where(u => u.RecordStatus == Domain.Models.Shared.RecordStatus.Deleted).ToList();

            if (customers.Count == 0)
            {
                result.AddError(ErrorCode.Ok, "No Privilege Data!");
                return result;
            }
            result.Payload = customers;
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
        }
        return result;
    }
}
