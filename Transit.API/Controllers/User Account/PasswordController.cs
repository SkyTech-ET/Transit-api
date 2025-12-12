
namespace Transit.Controllers;
public class PasswordController : BaseController
{
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordRequest request)
    {
        var command = request.Adapt<ChangePasswordCommand>();
        var result = await _mediator.Send(command);
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordRequest request)
    {
        var command = request.Adapt<ResetPasswordCommand>();
        var result = await _mediator.Send(command);
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(PasswordRequest request)
    {
        var command = request.Adapt<ForgotPasswordCommand>();
        var result = await _mediator.Send(command);
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }

}
