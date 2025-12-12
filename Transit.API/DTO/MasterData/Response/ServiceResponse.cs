using Transit.Domain.Models.Shared;

namespace Transit.API.DTO.MasterData;

public class ServiceResponse
{
    public long Id { get; set; }
    public string ServiceNumber { get; set; }
    public string ItemDescription { get; set; }
    public decimal DeclaredValue { get; set; }
    public string CountryOfOrigin { get; set; }
    public ServiceType ServiceType { get; set; }
    public long CustomerId { get; set; }
    public string CustomerName { get; set; }

    public long CreatedByUserId { get; set; }

    public List<ServiceStageResponse> Stages { get; set; } = new();
}

