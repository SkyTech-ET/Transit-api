using Microsoft.AspNetCore.Mvc;
using Transit.API.Services;
using Transit.Controllers;
using Transit.Domain.Models.Shared;
using Transit.Domain.Data;
using Transit.API.Helpers;
using Transit.Api.Contracts.MOT.Request;

namespace Transit.API.Controllers.MOT;

[ApiController]
[Route("api/v1/[controller]")]
public class MessagingController : BaseController
{
    private readonly IMessagingService _messagingService;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MessagingController(IMessagingService messagingService, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _messagingService = messagingService;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] Transit.Api.Contracts.MOT.Request.SendMessageRequest request)
    {
        try
        {
            // Get sender ID from JWT token
            var senderId = JwtHelper.GetCurrentUserId(_httpContextAccessor, _context);
            if (senderId == null)
                return Unauthorized("User not authenticated");

            var message = await _messagingService.SendMessageAsync(
                request.ServiceId,
                senderId.Value, // Use sender ID from token
                request.Subject,
                request.Content,
                request.Type,
                request.RecipientId
            );

            return HandleSuccessResponse(new { Message = "Message sent successfully", MessageId = message.Id });
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }

    [HttpGet("GetServiceMessages")]
    public async Task<IActionResult> GetServiceMessages([FromQuery] long serviceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var messages = await _messagingService.GetServiceMessagesAsync(serviceId, page, pageSize);
            return HandleSuccessResponse(messages);
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }

    [HttpGet("GetUserMessages")]
    public async Task<IActionResult> GetUserMessages([FromQuery] long userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var messages = await _messagingService.GetUserMessagesAsync(userId, page, pageSize);
            return HandleSuccessResponse(messages);
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }

    [HttpGet("GetMessageById")]
    public async Task<IActionResult> GetMessageById([FromQuery] long messageId)
    {
        try
        {
            var message = await _messagingService.GetMessageAsync(messageId);
            if (message == null)
                return NotFound("Message not found");

            return HandleSuccessResponse(message);
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }

    [HttpPost("MarkMessageAsRead")]
    public async Task<IActionResult> MarkMessageAsRead([FromBody] Transit.Api.Contracts.MOT.Request.MarkMessageAsReadRequest request)
    {
        try
        {
            await _messagingService.MarkMessageAsReadAsync(request.MessageId, request.UserId);
            return HandleSuccessResponse(new { Message = "Message marked as read" });
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }

    [HttpGet("GetUnreadMessages")]
    public async Task<IActionResult> GetUnreadMessages([FromQuery] long userId)
    {
        try
        {
            var messages = await _messagingService.GetUnreadMessagesAsync(userId);
            return HandleSuccessResponse(messages);
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }

    [HttpGet("GetUnreadCount")]
    public async Task<IActionResult> GetUnreadCount([FromQuery] long userId)
    {
        try
        {
            var count = await _messagingService.GetUnreadMessageCountAsync(userId);
            return HandleSuccessResponse(new { UnreadCount = count });
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }

    [HttpPost("SendGroupMessage")]
    public async Task<IActionResult> SendGroupMessage([FromBody] Transit.Api.Contracts.MOT.Request.SendGroupMessageRequest request)
    {
        try
        {
            await _messagingService.SendGroupMessageAsync(
                request.ServiceId,
                request.SenderId,
                request.Subject,
                request.Content,
                request.RecipientIds
            );

            return HandleSuccessResponse(new { Message = "Group message sent successfully" });
        }
        catch (Exception ex)
        {
            return HandleErrorResponse(ex);
        }
    }
}
