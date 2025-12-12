using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Transit.Api;
using Transit.Api.Contracts.User.Request;
using Transit.Api.Contracts.User.Response;
using Transit.Domain.Data;
using Transit.Domain.Models.Shared;

namespace Transit.Controllers;
public class UserController : BaseController
{
    private readonly IMediator _mediator;
    private readonly string _logoPath;
    private readonly string _documentsPath;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenHandlerService _tokenHandlerService;
    public UserController(IOptions<Settings> storageSettings, IMediator mediator, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, TokenHandlerService tokenHandlerService)
    {
        _mediator = mediator;
        _logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Profile_Photo");
        _documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "User_Documents");

        // Ensure the directories exist
        Directory.CreateDirectory(_logoPath);
        Directory.CreateDirectory(_documentsPath);
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm] UserRequest clientRequest)
    {
        // Validate the uploaded logo file
        if (clientRequest.ProfileFile == null || clientRequest.ProfileFile.Length == 0)
        {
            return BadRequest(new
            {
                Error = true,
                Message = "Photo file is required."
            });
        }
        // Set the path for saving the logo
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Profile_Photo");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        // Generate a unique name for the logo file
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(clientRequest.ProfileFile.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save the uploaded file to the server
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await clientRequest.ProfileFile.CopyToAsync(fileStream);
        }
        // Set the LogoPath in the request object
        clientRequest.ProfilePhoto = Path.Combine("Profile_Photo", uniqueFileName);



        var command = clientRequest.Adapt<CreateUserCommand>();
        var result = await _mediator.Send(command);
        var userDetail = result.Payload.Adapt<UserDetail>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }
    [HttpGet("db-test")]
    public async Task<IActionResult> DbTest()
    {
        try
        {
            // Replace Users with a table you have
            var count = await _context.Users.CountAsync();
            return Ok($"Connected to PostgreSQL! Users count: {count}");
        }
        catch (Exception ex)
        {
            return BadRequest($"DB error: {ex.Message}");
        }
    }

    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromForm] UpdateUserRequest clientRequest)
    {
        var userName = GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
        {
            userName = "";
        }
        // Fetch the current user including roles
        var currentUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Username == userName);

        if (currentUser == null)
        {
            return Unauthorized("User not found.");
        }

        // Check if current user is Admin
        bool isAdmin = currentUser.UserRoles.Any(r => r.Role.Name == "Admin");

        // If not admin and trying to update someone else's info — reject it
        if (!isAdmin && clientRequest.Id != currentUser.Id)
        {
            return Forbid("You are not allowed to update other users.");
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Profile_Photo");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var existingUser = await _context.Users
            .Where(x => x.Id == clientRequest.Id)
            .FirstOrDefaultAsync();

        if (existingUser == null)
        {
            return NotFound("User not found.");
        }

        string profilePhoto = existingUser.ProfilePhoto;

        // Update the logo if a new one is uploaded
        if (clientRequest.ProfileFile != null)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(clientRequest.ProfileFile.FileName);
            profilePhoto = Path.Combine("Profile_Photo", uniqueFileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await clientRequest.ProfileFile.CopyToAsync(fileStream);
            }
            // Update the logo path in the request object
            profilePhoto = profilePhoto;
        }
        clientRequest.ProfilePhoto = profilePhoto;


        var command = clientRequest.Adapt<UpdateUser>();
        var result = await _mediator.Send(command);
        var userDetail = result.Payload.Adapt<UserDetail>();

        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }
    private string GetCurrentUserName()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return null; // No token available in request
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var claims = _tokenHandlerService.GetClaims(token); // Use TokenHandlerService to get claims

        var userNameClaim = claims?.FirstOrDefault(c => c.Type == "userName");
        return userNameClaim?.Value; // Return the username or null if not found
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteUser(id));
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] UserLogin request)
    {
        var command = new LoginUserCommand { UserName = request.Username, Password = request.Password };
        var result = await _mediator.Send(command);
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);
    }

    [HttpPost("LogOut")]
    public IActionResult LogOut(string token)
    {
        //TODO Clear this Token from the session
        var retunrObject = new { Message = "Log out Successfully" };
        var json = JsonConvert.SerializeObject(retunrObject);
        return HandleSuccessResponse(json);
    }

    [HttpGet("GetAll/{recordStatus}")]
    public async Task<IActionResult> GetAll(RecordStatus? recordStatus)
    {
        var query = new GetAllUsersQuery { RecordStatus = recordStatus };
        var result = await _mediator.Send(query);

        if (result.IsError)
            return HandleErrorResponse(result.Errors);

        var usersList = result.Payload.Select(u => new UserDetail
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsSuperAdmin = u.IsSuperAdmin,
            IsAccountLocked = u.IsAccountLocked,
            LastLoginDateTime = u.LastLoginDateTime,
            PhoneNumber = u.Phone,
            LoginAttemptCount = u.LoginAttemptCount,
            RecordStatus = u.RecordStatus,
            IsConfirmationEmailSent = u.IsConfirmationEmailSent,
            VerificationToken = u.VerificationToken,

            // ?? Store Ids
         //   StoreIds = u.UserStores?.Select(s => s.StoreId).ToList() ?? new(),

            // ?? Roles mapping
            Roles = u.UserRoles?.Select(ur => new RoleDetail
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                Description = ur.Role.Description,
                RecordStatus = ur.Role.RecordStatus,
                Privileges = ur.Role.RolePrivileges?.Select(p => new PrivilegeDetail
                {
                    Id = p.Privilege.Id,
                    Action = p.Privilege.Action,
                    Description = p.Privilege.Description,
                    RecordStatus = p.Privilege.RecordStatus
                }).ToList()
            }).ToList(),

            // Store Detail (single)
            //UserStore = u.UserStores?.Select(s => new StoreDetail
            //{
            //    Id = s.Store.Id,
            //    Name = s.Store.Name,
            //    RecordStatus = s.Store.RecordStatus
            //}).FirstOrDefault()

        }).ToList();

        return HandleSuccessResponse(usersList); // ?? FIXED
    }


    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(long id)
    {
        var userResult = new UserDetail();
        var rolesList = new List<RoleDetail>();

        var query = new GetUserById(id);
        var result = await _mediator.Send(query);

        if (result.Payload == null)
        {
            // Return an empty array if no data is found
            return HandleSuccessResponse(new List<UserDetail>());
        }
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result.Payload);

    }


    [HttpGet("RefreshToken")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
        var result = await _mediator.Send(new RefreshToken(refreshToken));
        return result.IsError ? HandleErrorResponse(result.Errors) : HandleSuccessResponse(result);
    }

}
