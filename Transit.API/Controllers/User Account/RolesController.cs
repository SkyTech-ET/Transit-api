using Transit.Api;
using Transit.Api.Contracts.User.Response;
using Transit.Application.Handlers.UserAccount;
using Transit.Domain.Models.Shared;

namespace Transit.Controllers;
public class RolesController : BaseController
{
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] RoleRequest clientRequest)
    {
        var command = clientRequest.Adapt<CreateRoleCommand>();
        var result = await _mediator.Send(command);
        var roleDetail = result.Payload.Adapt<RoleDetail>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(roleDetail);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] RoleRequest clientRequest)
    {
        var command = clientRequest.Adapt<UpdateRole>();
        var result = await _mediator.Send(command);
        var roleDetail = result.Payload.Adapt<RoleDetail>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(roleDetail);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteRole(id));
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }

    [HttpGet("GetAll/{recordStatus}")]
    public async Task<IActionResult> GetAll(RecordStatus? recordStatus)
    {
        var query = new GetAllRolesQuery { RecordStatus = recordStatus };
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<List<RoleDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

    [HttpGet("GetRolesByUser/{userId}")]
    public async Task<IActionResult> GetRolesByUser(long userId)
    {
        var query = new GetRolesByUser(userId);
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<List<RoleDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

    [HttpGet("GetById/{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var roleResult = new RoleDetail();
        var privilegeList = new List<PrivilegeDetail>();

        var query = new GetRoleById(id);
        var result = await _mediator.Send(query);
        if (!result.IsError)
        {
            roleResult = result.Payload.Adapt<RoleDetail>();
            foreach (var item in result.Payload.RolePrivileges)
            {
                privilegeList.Add(item.Privilege.Adapt<PrivilegeDetail>());
            }
            roleResult.Privileges = privilegeList;

        }
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(roleResult);
    }


}