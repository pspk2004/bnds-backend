using backend.DTOs;

namespace backend.Services;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(int sellerId, CreateProductDto dto);
    Task<ProductDto> GetProductByIdAsync(int id);
    Task<PagedResultDto<ProductDto>> GetProductsAsync(ProductFilterDto filter);
    Task<ProductDto> UpdateProductAsync(int productId, int sellerId, UpdateProductDto dto);
    Task DeleteProductAsync(int productId, int sellerId);
    Task<ProductDto> ToggleFeatureAsync(int productId, int userId, bool isAdmin);
    Task ForceCloseAuctionAsync(int productId);
    Task<List<ProductDto>> GetUserProductsAsync(int userId);
}
