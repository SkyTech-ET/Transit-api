using Transit.Api;
using Transit.Domain.Models.Shared;


namespace Transit.Controllers;
public class RolePrivilegeController : BaseController
{
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] RolePrivilegeRequest clientRequest)
    {
        var command = clientRequest.Adapt<AddRolePrivilege>();
        var result = await _mediator.Send(command);
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }


    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(RecordStatus? recordStatus)
    {
        var query = new GetAllRolePrivilegesQuery { RecordStatus = recordStatus };
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<List<RolePrivilegeDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

}