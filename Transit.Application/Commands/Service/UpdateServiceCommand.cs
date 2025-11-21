using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

public class UpdateServiceCommand : IRequest<OperationResult<Service>>
{
    public long Id { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public string RouteCategory { get; set; } = string.Empty;
    public decimal DeclaredValue { get; set; }
    public string TaxCategory { get; set; } = string.Empty;
    public string CountryOfOrigin { get; set; } = string.Empty;
    public RiskLevel? RiskLevel { get; set; }
}

