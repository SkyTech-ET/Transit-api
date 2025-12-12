using Transit.Domain.Models.Shared;

namespace Transit.Application;
public class GetAllUsersQuery : IRequest<OperationResult<List<User>>>
{
    public RecordStatus? RecordStatus { get; set; }
}
