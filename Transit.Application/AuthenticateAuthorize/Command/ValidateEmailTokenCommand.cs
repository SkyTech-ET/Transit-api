namespace Transit.Application.AuthenticateAuthorize.Command;

public class ValidateEmailTokenCommand : IRequest<OperationResult<bool>>
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
internal class ValidatePasswordTokenHandler : IRequestHandler<ValidateEmailTokenCommand, OperationResult<bool>>
{
    private readonly TokenHandlerService _tokenHandlerService;
    public ValidatePasswordTokenHandler(TokenHandlerService _tokenHandlerService) => this._tokenHandlerService = _tokenHandlerService;
    public async Task<OperationResult<bool>> Handle(ValidateEmailTokenCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();
        try
        {
            if (!_tokenHandlerService.ValidateToken(request.Token)) //decrypt and validate access token
            {
                result.Payload = false;
                return result;
            }
            var claims = _tokenHandlerService.GetClaims(request.Token); // get claims from the tokene
            if (claims.Exists(x => x.Type == "email" && x.Value.Trim().ToLower() == request.Email.Trim().ToLower()) &&
                claims.Exists(x => x.Type == "name" && x.Value.Trim().ToLower() == request.Name.Trim().ToLower()))
            {
                result.Payload = true;
                return result;
            }
            result.Payload = false;
            return result;

        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.ServerError, ex.Message);
            return result;
        }
        return result;
    }
}
