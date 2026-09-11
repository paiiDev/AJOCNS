using AJOCNS.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ajocns.database.interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string? email);
        Task<User?> GetUserByEmailForEditAsync(string email);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<GraduationRecord?> GetGraduationRecordByGrnOnlyAsync(string grn);
        Task<bool> CreateUserAsync(User user);
        Task<List<Company>> GetCompaniesAsync();
        Task<List<Position>> GetPositionsAsync();
        Task<bool> CompanyExistsAsync(int companyId);
        Task<bool> PositionExistsAsync(int positionId);
        Task<Company> GetOrCreateCompanyAsync(string companyName);
        Task<Position> GetOrCreatePositionAsync(string positionName);
        Task<List<User>> GetPendingUsersAsync();
        Task<List<User>> GetExternalPartnersAsync();
        Task<bool> IsExternalPartnerAsync(int userId);
        Task<User?> GetPendingUserByIdAsync(int userId);
        Task<bool> UpdateUserStatusAsync(int userId, string status);
    }
}