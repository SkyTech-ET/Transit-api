using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

internal class GetAllServicesQueryHandler : IRequestHandler<GetAllServicesQuery, OperationResult<ServiceListResponse>>
{
    private readonly ApplicationDbContext _context;

    public GetAllServicesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<ServiceListResponse>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<ServiceListResponse>();

        var query = _context.Services
            .Include(s => s.Customer)
            .Include(s => s.AssignedCaseExecutor)
            .Include(s => s.AssignedAssessor)
            .Include(s => s.CreatedByDataEncoder)
            .Include(s => s.Stages)
            .Include(s => s.Documents)
            .AsQueryable();

        // Apply filters
        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.ServiceType.HasValue)
            query = query.Where(s => s.ServiceType == request.ServiceType.Value);

        if (request.RiskLevel.HasValue)
            query = query.Where(s => s.RiskLevel == request.RiskLevel.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(s => s.CustomerId == request.CustomerId.Value);

        if (request.CaseExecutorId.HasValue)
            query = query.Where(s => s.AssignedCaseExecutorId == request.CaseExecutorId.Value);

        if (request.AssessorId.HasValue)
            query = query.Where(s => s.AssignedAssessorId == request.AssessorId.Value);

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(s => 
                s.ServiceNumber.Contains(request.Search) ||
                s.ItemDescription.Contains(request.Search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var services = await query
            .OrderByDescending(s => s.RegisteredDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new ServiceListResponse
        {
            Data = services,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        result.Payload = paginatedResult;
        result.Message = "Operation success";
        return result;
    }
}

