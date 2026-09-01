using AJOCNS.Database.Context;
using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Database.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<string> GetUserNameAsync(int userId, string role)
        {
            if (role == "Admin")
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UserId == userId);
                return admin?.Name ?? string.Empty;
            }
            else if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                return student?.Name ?? "Student";
            }
            else if (role == "Mentor")
            {
                var mentor = await _context.Mentors.FirstOrDefaultAsync(m => m.UserId == userId);
                return mentor?.Name ?? string.Empty;
            }
            else if (role == "ExternalPartner")
            {
                var externalPartner = await _context.ExternalPartners.FirstOrDefaultAsync(ep => ep.UserId == userId);
                return externalPartner?.Name ?? string.Empty;
            }
            else
            {
                throw new ArgumentException("Invalid role specified.");
            }
        }
    }
}
