using Transit.Domain.Models.MOT;

namespace Transit.Application;

public class DeleteServiceCommand : IRequest<OperationResult<bool>>
{
    public long Id { get; set; }
}

