using Transit.Api.Contracts.Common;
using System.Net;
using Error = Transit.Application.Error;

namespace Transit.Controllers;


[Route("api/v1/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    private IMediator _mediatorInstance;
    protected IMediator _mediator => _mediatorInstance ??= HttpContext.RequestServices.GetService<IMediator>();

    protected IActionResult HandleSuccessResponse<T>(T data, string message = "Operation Success")
    {
        var apiResponse = new ApiResponse<T>();

        apiResponse.Error = false;
        apiResponse.StatusCode = (int)HttpStatusCode.OK;
        apiResponse.Response = new Response<T>()
        {
            Data = data
        };
        apiResponse.Message = message;

        return Ok(apiResponse);
    }

    protected IActionResult HandleErrorResponse<T>(T data, ErrorCode code, string message)
    {
        var apiResponse = new ApiResponse<T>();
        apiResponse.Error = true;
        apiResponse.StatusCode = (int)code;
        apiResponse.Response = new Response<T>()
        {
            Data = data
        };
        apiResponse.Message = message;

        return StatusCode((int)code, apiResponse);
    }

    protected IActionResult HandleErrorResponse<T>(T data)
    {
        var errors = data.Adapt<List<Error>>();

        if (!errors.Any())
            errors.Add(new Error()
            {
                Code = ErrorCode.ServerError,
                Message = "An error occurred while processing your request."
            });
        var error = errors[0];
        var apiResponse = new ApiResponse<T>();

        if (errors.Any(e => e.Code == ErrorCode.NotFound))
        {
            error = errors.Find(e => e.Code == ErrorCode.NotFound);
            apiResponse.Error = true;
            apiResponse.Errors.Add(error.Message);
            apiResponse.StatusCode = (int)error.Code;
            apiResponse.Response = null;
            apiResponse.Message = error.Message;
            return StatusCode((int)error.Code, apiResponse);

        }

        if (errors.Any(e => e.Code == ErrorCode.UnAuthorized))
        {
            error = errors.First(e => e.Code == ErrorCode.UnAuthorized);
            apiResponse.Error = true;
            apiResponse.Errors.Add(error.Message);
            apiResponse.StatusCode = (int)error.Code;
            apiResponse.Response = null;
            apiResponse.Message = error.Message;
            return StatusCode((int)error.Code, apiResponse);
        }

        apiResponse.Error = true;
        apiResponse.Errors.Add(error.Message);
        apiResponse.StatusCode = 200;
        apiResponse.Response = null;
        apiResponse.Message = error.Message;
        return StatusCode(200, apiResponse);
    }
    protected IActionResult HandleTokenErrorResponse(List<Error> errors)
    {
        var clientStatus = string.Empty;
        var apiError = new OperationResult<UserTokenValidationResponse>();

        if (errors.Any(e => e.Code == ErrorCode.ServerError))
        {
            var error = errors.FirstOrDefault(e => e.Code == ErrorCode.ServerError);

            apiError.Message = "Server error";
            apiError.AddError(ErrorCode.ServerError, "Server error");
            return StatusCode(500, apiError);
        }
        clientStatus = errors.Any(x => x.Message == "User is not Authorized to access.") ? "104" : clientStatus;
        clientStatus = errors.Any(x => x.Message == "Id token is invalid.") ? "103" : clientStatus;
        clientStatus = errors.Any(x => x.Message == "Client is not Authorized.") ? "102" : clientStatus;
        clientStatus = errors.Any(x => x.Message == "Client token is invalid.") ? "101" : clientStatus;
        apiError.Message = clientStatus;
        errors.ForEach(e => apiError.AddError(ErrorCode.ServerError, e.Message));
        return StatusCode(401, apiError);
    }
}
