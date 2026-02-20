using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get current user's profile with membership usage stats
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), 200)]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(GetUserId());
        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), 200)]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a specific user (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Suspend a user (Admin only)
    /// </summary>
    [HttpPost("suspend")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SuspendUser([FromBody] SuspendUserDto dto)
    {
        await _userService.SuspendUserAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("User suspended successfully."));
    }

    /// <summary>
    /// Unsuspend a user (Admin only)
    /// </summary>
    [HttpPut("{id}/unsuspend")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UnsuspendUser(int id)
    {
        await _userService.UnsuspendUserAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("User unsuspended successfully."));
    }

    /// <summary>
    /// Pay penalty to lift suspension
    /// </summary>
    [HttpPost("pay-penalty")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PayPenalty([FromBody] PayPenaltyDto dto)
    {
        var result = await _userService.PayPenaltyAsync(GetUserId(), dto);
        return Ok(ApiResponse<TransactionDto>.SuccessResponse(result, "Penalty paid. Suspension lifted."));
    }

    /// <summary>
    /// Get current user's notifications
    /// </summary>
    [HttpGet("notifications")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), 200)]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _userService.GetNotificationsAsync(GetUserId());
        return Ok(ApiResponse<List<NotificationDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("notifications/{id}/read")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        await _userService.MarkNotificationReadAsync(id, GetUserId());
        return Ok(ApiResponse<string>.SuccessResponse("Notification marked as read."));
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("notifications/read-all")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> MarkAllRead()
    {
        await _userService.MarkAllNotificationsReadAsync(GetUserId());
        return Ok(ApiResponse<string>.SuccessResponse("All notifications marked as read."));
    }
}
