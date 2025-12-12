namespace Transit.Application;

public class LoginUserCommand : IRequest<OperationResult<UserLoginDto>>
{
    public string UserName { get; set; }
    public string Password { get; set; }
}
