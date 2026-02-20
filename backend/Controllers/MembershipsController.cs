using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipsController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get all available membership plans
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<MembershipDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _membershipService.GetAllMembershipsAsync();
        return Ok(ApiResponse<List<MembershipDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a specific membership plan
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MembershipDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _membershipService.GetMembershipByIdAsync(id);
        return Ok(ApiResponse<MembershipDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Purchase a membership plan
    /// </summary>
    [HttpPost("purchase")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Purchase([FromBody] PurchaseMembershipDto dto)
    {
        var result = await _membershipService.PurchaseMembershipAsync(GetUserId(), dto);
        return Ok(ApiResponse<TransactionDto>.SuccessResponse(result, "Membership purchased successfully."));
    }
}
