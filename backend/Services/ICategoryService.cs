using backend.DTOs;

namespace backend.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(int id, CreateCategoryDto dto);
    Task DeleteCategoryAsync(int id);
}
