
namespace Transit.Application;

public record CreatePrivilegeCommand(string Action, string Description) : IRequest<OperationResult<Privilege>>;

