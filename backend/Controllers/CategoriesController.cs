using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a specific category
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Create a new category (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<CategoryDto>.SuccessResponse(result, "Category created successfully."));
    }

    /// <summary>
    /// Update a category (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(result, "Category updated successfully."));
    }

    /// <summary>
    /// Delete a category (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("Category deleted successfully."));
    }
}
