using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.IsInRole("Admin");

    /// <summary>
    /// Get products with filtering, sorting, and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<ProductDto>>), 200)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetProductsAsync(filter);
        return Ok(ApiResponse<PagedResultDto<ProductDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a single product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Create a new product listing (auction)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(GetUserId(), dto);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id },
            ApiResponse<ProductDto>.SuccessResponse(result, "Product created successfully."));
    }

    /// <summary>
    /// Update a product (only if no active bids)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, GetUserId(), dto);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(result, "Product updated successfully."));
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id, GetUserId());
        return Ok(ApiResponse<string>.SuccessResponse("Product deleted successfully."));
    }

    /// <summary>
    /// Toggle featured status on a product
    /// </summary>
    [HttpPut("{id}/feature")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ToggleFeature(int id)
    {
        var result = await _productService.ToggleFeatureAsync(id, GetUserId(), IsAdmin());
        return Ok(ApiResponse<ProductDto>.SuccessResponse(result, "Feature status toggled."));
    }

    /// <summary>
    /// Force close an auction (Admin only)
    /// </summary>
    [HttpPut("{id}/force-close")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ForceCloseAuction(int id)
    {
        await _productService.ForceCloseAuctionAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("Auction force-closed successfully."));
    }

    /// <summary>
    /// Get products listed by the current user
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "User,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), 200)]
    public async Task<IActionResult> GetMyProducts()
    {
        var result = await _productService.GetUserProductsAsync(GetUserId());
        return Ok(ApiResponse<List<ProductDto>>.SuccessResponse(result));
    }
}
