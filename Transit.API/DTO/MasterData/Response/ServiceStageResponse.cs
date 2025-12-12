using Transit.Domain.Models.Shared;

namespace Transit.API.DTO.MasterData;

public class ServiceStageResponse
{
    public long Id { get; set; }
    public ServiceStage Stage { get; set; }
    public DateTime CreatedDate { get; set; }
    public RecordStatus Status { get; set; }
}
