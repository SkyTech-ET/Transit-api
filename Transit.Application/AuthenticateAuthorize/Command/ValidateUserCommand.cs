namespace Transit.Application;

public class ValidateUserCommand : IRequest<OperationResult<UserTokenValidationResponse>>
{
    public string AccessToken { get; set; }
    public string ApiResource { get; set; }
}
