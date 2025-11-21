using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.MOT.Request;

public class SetRiskLevelRequest
{
    public long ServiceId { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string? RiskNotes { get; set; }
}

