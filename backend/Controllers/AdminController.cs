using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _userService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all transactions
    /// </summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<List<TransactionDto>>), 200)]
    public async Task<IActionResult> GetTransactions()
    {
        var result = await _userService.GetAllTransactionsAsync();
        return Ok(ApiResponse<List<TransactionDto>>.SuccessResponse(result));
    }
}
