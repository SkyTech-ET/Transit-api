using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class GetAllServicesQuery : IRequest<OperationResult<ServiceListResponse>>
{
    public ServiceStatus? Status { get; set; }
    public ServiceType? ServiceType { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public long? CustomerId { get; set; }
    public long? CaseExecutorId { get; set; }
    public long? AssessorId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ServiceListResponse
{
    public List<Service> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

