using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.MOT.Request;

public class UpdateStageStatusRequest
{
    //public long ServiceId { get; set; }
    public long StageId { get; set; }
    public StageStatus Status { get; set; }
    public string? Comments { get; set; }
}

