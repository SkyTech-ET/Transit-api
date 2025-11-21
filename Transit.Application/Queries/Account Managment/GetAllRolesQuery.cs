using Transit.Domain.Models.Shared;

namespace Transit.Application;
public class GetAllRolesQuery : IRequest<OperationResult<List<Role>>>
{
    public RecordStatus? RecordStatus { get; set; }
}
