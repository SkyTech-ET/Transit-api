namespace Transit.Api.Contracts.MOT.Request;

public class BlockStageRequest
{
    public long ServiceId { get; set; }
    public long StageId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

