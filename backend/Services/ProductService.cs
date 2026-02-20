using AutoMapper;
using backend.DTOs;
using backend.Models;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IMapper _mapper;

    public ProductService(
        IRepository<Product> productRepo,
        IRepository<User> userRepo,
        IRepository<Category> categoryRepo,
        IRepository<Notification> notificationRepo,
        IMapper mapper)
    {
        _productRepo = productRepo;
        _userRepo = userRepo;
        _categoryRepo = categoryRepo;
        _notificationRepo = notificationRepo;
        _mapper = mapper;
    }

    public async Task<ProductDto> CreateProductAsync(int sellerId, CreateProductDto dto)
    {
        var seller = await _userRepo.Query()
            .Include(u => u.Membership)
            .FirstOrDefaultAsync(u => u.Id == sellerId)
            ?? throw new KeyNotFoundException("User not found.");

        if (seller.IsSuspended)
            throw new InvalidOperationException("Your account is suspended. You cannot post products.");

        // Check membership ad limit
        var membership = seller.Membership;
        if (membership != null && membership.MaxAds != -1)
        {
            var activeAdsCount = await _productRepo.CountAsync(p => p.SellerId == sellerId && !p.IsClosed);
            if (activeAdsCount >= membership.MaxAds)
                throw new InvalidOperationException($"You have reached your ad limit ({membership.MaxAds}). Upgrade your membership to post more ads.");
        }

        var categoryExists = await _categoryRepo.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            throw new KeyNotFoundException("Category not found.");

        var product = new Product
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            SellerId = sellerId,
            StartingPrice = dto.StartingPrice,
            CurrentPrice = dto.StartingPrice,
            BidEndTime = dto.BidEndTime,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id);
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var product = await _productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Winner)
            .Include(p => p.Bids)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Product not found.");

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<PagedResultDto<ProductDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Winner)
            .Include(p => p.Bids)
            .AsQueryable();

        // Filters
        if (filter.Category.HasValue)
            query = query.Where(p => p.CategoryId == filter.Category.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.CurrentPrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.CurrentPrice <= filter.MaxPrice.Value);

        if (filter.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

        if (filter.EndingSoon.HasValue && filter.EndingSoon.Value)
            query = query.Where(p => !p.IsClosed && p.BidEndTime <= DateTime.UtcNow.AddHours(24));

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Title.Contains(filter.Search) || p.Description.Contains(filter.Search));

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "priceasc" => query.OrderBy(p => p.CurrentPrice),
            "pricedesc" => query.OrderByDescending(p => p.CurrentPrice),
            "endingsoon" => query.Where(p => !p.IsClosed).OrderBy(p => p.BidEndTime),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResultDto<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(items),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ProductDto> UpdateProductAsync(int productId, int sellerId, UpdateProductDto dto)
    {
        var product = await _productRepo.GetByIdAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (product.SellerId != sellerId)
            throw new UnauthorizedAccessException("You can only edit your own products.");

        if (product.IsClosed)
            throw new InvalidOperationException("Cannot edit a closed auction.");

        var hasBids = await _productRepo.Query()
            .Where(p => p.Id == productId)
            .SelectMany(p => p.Bids)
            .AnyAsync(b => !b.IsWithdrawn);

        if (hasBids)
            throw new InvalidOperationException("Cannot edit a product that already has active bids.");

        product.Title = dto.Title;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id);
    }

    public async Task DeleteProductAsync(int productId, int sellerId)
    {
        var product = await _productRepo.GetByIdAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (product.SellerId != sellerId)
            throw new UnauthorizedAccessException("You can only delete your own products.");

        if (product.IsClosed)
            throw new InvalidOperationException("Cannot delete a closed auction.");

        _productRepo.Remove(product);
        await _productRepo.SaveChangesAsync();
    }

    public async Task<ProductDto> ToggleFeatureAsync(int productId, int userId, bool isAdmin)
    {
        var product = await _productRepo.GetByIdAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (isAdmin)
        {
            product.IsFeatured = !product.IsFeatured;
        }
        else
        {
            if (product.SellerId != userId)
                throw new UnauthorizedAccessException("You can only feature your own products.");

            if (product.IsFeatured)
            {
                product.IsFeatured = false;
            }
            else
            {
                var user = await _userRepo.Query()
                    .Include(u => u.Membership)
                    .FirstOrDefaultAsync(u => u.Id == userId)
                    ?? throw new KeyNotFoundException("User not found.");

                var membership = user.Membership;
                if (membership == null || membership.MaxFeaturedAds == 0)
                    throw new InvalidOperationException("Your membership does not allow featured ads. Upgrade your membership.");

                if (membership.MaxFeaturedAds != -1)
                {
                    var featuredCount = await _productRepo.CountAsync(p => p.SellerId == userId && p.IsFeatured && !p.IsClosed);
                    if (featuredCount >= membership.MaxFeaturedAds)
                        throw new InvalidOperationException($"You have reached your featured ad limit ({membership.MaxFeaturedAds}).");
                }

                product.IsFeatured = true;
            }
        }

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id);
    }

    public async Task ForceCloseAuctionAsync(int productId)
    {
        var product = await _productRepo.Query()
            .Include(p => p.Bids)
                .ThenInclude(b => b.User)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (product.IsClosed)
            throw new InvalidOperationException("Auction is already closed.");

        product.IsClosed = true;

        var highestBid = product.Bids
            .Where(b => !b.IsWithdrawn)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefault();

        if (highestBid != null)
        {
            product.WinnerId = highestBid.UserId;
            product.CurrentPrice = highestBid.Amount;

            var notification = new Notification
            {
                UserId = highestBid.UserId,
                Title = "Auction Won (Force Closed)",
                Message = $"The auction for \"{product.Title}\" was force-closed by admin. You are the winner with a bid of ?{highestBid.Amount:N2}.",
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepo.AddAsync(notification);
        }

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();
    }

    public async Task<List<ProductDto>> GetUserProductsAsync(int userId)
    {
        var products = await _productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Winner)
            .Include(p => p.Bids)
            .Where(p => p.SellerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ProductDto>>(products);
    }
}
