using Transit.Domain.Models.Shared;

namespace Transit.Application;
public class GetAllPrivilegesQuery : IRequest<OperationResult<List<Privilege>>>
{
    public RecordStatus? RecordStatus { get; set; }
}
