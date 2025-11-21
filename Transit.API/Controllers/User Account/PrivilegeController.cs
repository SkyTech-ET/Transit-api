using Transit.Api;
using Transit.Application.Handlers.UserAccount;
using Transit.Domain.Models.Shared;
using System.Reflection;


namespace Transit.Controllers;
public class PrivilegeController : BaseController
{
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] PrivilegeRequest request)
    {
        var command = request.Adapt<CreatePrivilegeCommand>();
        var result = await _mediator.Send(command);
        var roleDetail = result.Payload.Adapt<PrivilegeDetail>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(roleDetail);
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] PrivilegeRequest request)
    {
        var command = request.Adapt<UpdatePrivilege>();
        var result = await _mediator.Send(command);
        var roleDetail = result.Payload.Adapt<PrivilegeDetail>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(roleDetail);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeletePrivilege(id));
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }


    [HttpGet("GetAll/{recordStatus}")]
    public async Task<IActionResult> GetAll(RecordStatus? recordStatus)
    {
        var query = new GetAllPrivilegesQuery { RecordStatus = recordStatus };
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<List<PrivilegeDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

    [HttpGet("GetByRoleId")]
    public async Task<IActionResult> GetByRoleId(long roleId)
    {
        var query = new GetPrivilegeByRoleQuery(roleId);
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<List<PrivilegeDetail>>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(long Id)
    {
        var query = new GetPrivilegeById(Id);
        var result = await _mediator.Send(query);
        var rolesList = result.Payload.Adapt<PrivilegeDetail>();
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(rolesList);
    }

    [HttpPost(nameof(SeedPrivileges))]
    public async Task<IActionResult> SeedPrivileges()
    {
        List<PrivilegeDto> privilaegs = new List<PrivilegeDto>();
        Assembly asm = Assembly.GetExecutingAssembly();

        var controlleractionlist = asm.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                .Select(x => new { Controller = x.DeclaringType.Name, Action = x.Name, ReturnType = x.ReturnType.Name, Attributes = String.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", ""))) })
                .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();


        foreach (var controlleraction in controlleractionlist)
        {
            var priveldge = new PrivilegeDto
            {
                Action = controlleraction.Controller.Replace("Controller", "") + "-" + controlleraction.Action,
                Description = controlleraction.Controller.Replace("Controller", "")
            };
            privilaegs.Add(priveldge);
        }
        var command = new PrivilegeSeeder(privilaegs);
        var result = await _mediator.Send(command);
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }

}