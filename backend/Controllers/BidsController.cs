using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;

    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Place a bid on a product
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(ApiResponse<BidDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PlaceBid([FromBody] CreateBidDto dto)
    {
        var result = await _bidService.PlaceBidAsync(GetUserId(), dto);
        return CreatedAtAction(nameof(GetBidsByProduct), new { productId = result.ProductId },
            ApiResponse<BidDto>.SuccessResponse(result, "Bid placed successfully."));
    }

    /// <summary>
    /// Withdraw a bid (max 3 times before suspension)
    /// </summary>
    [HttpPut("{id}/withdraw")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> WithdrawBid(int id)
    {
        await _bidService.WithdrawBidAsync(id, GetUserId());
        return Ok(ApiResponse<string>.SuccessResponse("Bid withdrawn successfully."));
    }

    /// <summary>
    /// Get all bids for a specific product
    /// </summary>
    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(ApiResponse<List<BidDto>>), 200)]
    public async Task<IActionResult> GetBidsByProduct(int productId)
    {
        var result = await _bidService.GetBidsByProductAsync(productId);
        return Ok(ApiResponse<List<BidDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all bids placed by the current user
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<BidDto>>), 200)]
    public async Task<IActionResult> GetMyBids()
    {
        var result = await _bidService.GetBidsByUserAsync(GetUserId());
        return Ok(ApiResponse<List<BidDto>>.SuccessResponse(result));
    }
}
