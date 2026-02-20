using backend.DTOs;

namespace backend.Services;

public interface IMembershipService
{
    Task<List<MembershipDto>> GetAllMembershipsAsync();
    Task<MembershipDto> GetMembershipByIdAsync(int id);
    Task<TransactionDto> PurchaseMembershipAsync(int userId, PurchaseMembershipDto dto);
}
