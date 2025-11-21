using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.MOT.Request;

public class CreateServiceRequest
{
    public long CustomerId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public string RouteCategory { get; set; } = string.Empty;
    public decimal DeclaredValue { get; set; }
    public string TaxCategory { get; set; } = string.Empty;
    public string CountryOfOrigin { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
}
