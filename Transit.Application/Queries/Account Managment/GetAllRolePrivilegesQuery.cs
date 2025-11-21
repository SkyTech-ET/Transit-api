using Transit.Domain.Models.Shared;

namespace Transit.Application;
public class GetAllRolePrivilegesQuery : IRequest<OperationResult<List<RolePrivilege>>>
{
    public RecordStatus? RecordStatus { get; set; }
}
