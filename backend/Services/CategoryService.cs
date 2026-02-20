using AutoMapper;
using backend.DTOs;
using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IMapper _mapper;

    public CategoryService(IRepository<Category> categoryRepo, IRepository<Product> productRepo, IMapper mapper)
    {
        _categoryRepo = categoryRepo;
        _productRepo = productRepo;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var exists = await _categoryRepo.AnyAsync(c => c.Name == dto.Name);
        if (exists)
            throw new InvalidOperationException("A category with this name already exists.");

        var category = _mapper.Map<Category>(dto);
        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, CreateCategoryDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        category.Name = dto.Name;
        category.ImageUrl = dto.ImageUrl;
        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        var hasProducts = await _productRepo.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete a category that has products.");

        _categoryRepo.Remove(category);
        await _categoryRepo.SaveChangesAsync();
    }
}
