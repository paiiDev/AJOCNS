using AJOCNS.Database.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ajocns.database.interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string? email);
        Task<bool> EmailExistsAsync(string email);
        Task<GraduationRecord?> GetGraduationRecordByGrnOnlyAsync(string grn);
        Task<bool> CreateUserAsync(User user);
        Task<List<Company>> GetCompaniesAsync();
        Task<List<Position>> GetPositionsAsync();
        Task<bool> CompanyExistsAsync(int companyId);
        Task<bool> PositionExistsAsync(int positionId);
        Task<List<User>> GetPendingUsersAsync();
        Task<User?> GetPendingUserByIdAsync(int userId);
        Task<bool> UpdateUserStatusAsync(int userId, string status);
    }
}