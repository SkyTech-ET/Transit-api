using Transit.Domain.Models.MOT;

namespace Transit.Application;

public class GetServiceByIdQuery : IRequest<OperationResult<Service>>
{
    public long Id { get; set; }
}

