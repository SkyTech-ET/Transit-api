
namespace Transit.Application;

public record CreateRoleCommand(string Name, string Description, List<long> Privileges) : IRequest<OperationResult<Role>>;

