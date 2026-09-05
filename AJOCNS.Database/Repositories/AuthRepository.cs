using ajocns.database.interfaces;
using AJOCNS.Database.Context;
using AJOCNS.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AJOCNS.Database.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string? email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email);
        }

        public async Task<GraduationRecord?> GetGraduationRecordByGrnOnlyAsync(string grn)
        {
            return await _context.GraduationRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Grn == grn);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Company>> GetCompaniesAsync()
        {
            return await _context.Companies.AsNoTracking().OrderBy(c => c.CompanyName).ToListAsync();
        }

        public async Task<List<Position>> GetPositionsAsync()
        {
            return await _context.Positions.AsNoTracking().OrderBy(p => p.Position1).ToListAsync();
        }

        public async Task<bool> CompanyExistsAsync(int companyId)
        {
            return await _context.Companies.AnyAsync(c => c.CompanyId == companyId);
        }

        public async Task<bool> PositionExistsAsync(int positionId)
        {
            return await _context.Positions.AnyAsync(p => p.PositionId == positionId);
        }

        public async Task<List<User>> GetPendingUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Mentor)
                .Include(u => u.ExternalPartner)
                .Where(u => u.Status == "Pending")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetPendingUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Mentor)
                .Include(u => u.ExternalPartner)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Status == "Pending");
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, string status)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user is null) return false;

                user.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}