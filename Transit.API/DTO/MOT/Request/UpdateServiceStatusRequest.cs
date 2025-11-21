using Transit.Domain.Models.Shared;

namespace Transit.Api.Contracts.MOT.Request;

public class UpdateServiceStatusRequest
{
    public long Id { get; set; }
    public ServiceStatus Status { get; set; }
}

