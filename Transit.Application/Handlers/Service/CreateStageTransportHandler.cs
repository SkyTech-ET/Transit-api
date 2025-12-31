using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Transit.Domain;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;

namespace Transit.Application;

internal class CreateStageTransportHandler : IRequestHandler<CreateStageTransportCommand, OperationResult<StageTransport>>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession? _session => _httpContextAccessor.HttpContext?.Session;
    private readonly TokenHandlerService _tokenHandlerService;

    public CreateStageTransportHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        TokenHandlerService tokenHandlerService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandlerService = tokenHandlerService;
    }

    public async Task<OperationResult<StageTransport>> Handle(CreateStageTransportCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<StageTransport>();
        var userName = GetCurrentUserName();
        if (string.IsNullOrEmpty(userName))
        {
            userName = "";
        }
        long userId = 0;
        if (userName.Length > 0)
        {
            var existingUsers = await _context.Users
                      .FirstOrDefaultAsync(x => x.Username == userName);
            if (existingUsers != null)
                userId = existingUsers.Id;
        }

        // Create service entity
        var stageTransport = StageTransport.Create(
            request.FullName,
            request.PlateNumber,
            request.PhoneNumber,
            request.LicenceDocument,
            request.ProductAmount,
            request.ServiceStageId
        );

        _context.StageTransports.Add(stageTransport);
        await _context.SaveChangesAsync(cancellationToken);

        result.Payload = stageTransport;
        result.Message = "Operation success";


        return result;
    }

    private string? GetCurrentUserName()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return null;

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var claims = _tokenHandlerService.GetClaims(token);
        var userNameClaim = claims?.FirstOrDefault(c => c.Type == "userName");
        return userNameClaim?.Value;
    }
}


