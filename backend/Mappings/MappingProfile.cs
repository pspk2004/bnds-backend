using AutoMapper;
using backend.DTOs;
using backend.Models;

namespace backend.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(d => d.MembershipName, opt => opt.MapFrom(s => s.Membership != null ? s.Membership.Name : null));

        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SellerName, opt => opt.MapFrom(s => s.Seller.Name))
            .ForMember(d => d.WinnerName, opt => opt.MapFrom(s => s.Winner != null ? s.Winner.Name : null))
            .ForMember(d => d.TotalBids, opt => opt.MapFrom(s => s.Bids.Count(b => !b.IsWithdrawn)));

        CreateMap<CreateProductDto, Product>();

        // Bid mappings
        CreateMap<Bid, BidDto>()
            .ForMember(d => d.ProductTitle, opt => opt.MapFrom(s => s.Product.Title))
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name));

        // Membership mappings
        CreateMap<Membership, MembershipDto>();

        // Transaction mappings
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name));

        // Category mappings
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();

        // Notification mappings
        CreateMap<Notification, NotificationDto>();
    }
}
