using backend.DTOs;

namespace backend.Services;

public interface IBidService
{
    Task<BidDto> PlaceBidAsync(int userId, CreateBidDto dto);
    Task WithdrawBidAsync(int bidId, int userId);
    Task<List<BidDto>> GetBidsByProductAsync(int productId);
    Task<List<BidDto>> GetBidsByUserAsync(int userId);
}
