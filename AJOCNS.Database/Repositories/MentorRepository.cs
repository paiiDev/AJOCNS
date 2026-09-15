using AJOCNS.Database.Context;
using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AJOCNS.Database.Repositories
{
    public class MentorRepository : IMentorRepository
    {
        private readonly AppDbContext _context;

        public MentorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Mentor>> GetAllMentorsAsync()
        {
            return await _context.Mentors
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Company)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Position)
                .Where(m => m.User.Status != "Rejected")
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<(List<Mentor> Items, int TotalCount)> GetMentorsPagedAsync(int page, int pageSize, string? search)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Mentors
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Company)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Position)
                .Where(m => m.User.Status == "Active" || m.User.Status == "Inactive");

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(m =>
                    m.Name.Contains(term) ||
                    m.User.Email.Contains(term) ||
                    (m.Expertise != null && m.Expertise.Contains(term)) ||
                    (m.AlumniGrn != null && m.AlumniGrn.Contains(term)));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Mentor?> GetMentorByUserIdAsync(int userId)
        {
            return await _context.Mentors
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Company)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Position)
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }

        public async Task<Mentor?> GetMentorByIdAsync(int mentorId)
        {
            return await _context.Mentors
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Company)
                .Include(m => m.EmploymentRecords)
                    .ThenInclude(er => er.Position)
                .FirstOrDefaultAsync(m => m.MentorId == mentorId);
        }

        public async Task<bool> UpdateMentorAsync(Mentor mentor)
        {
            try
            {
                _context.Mentors.Update(mentor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<EmploymentRecord>> GetEmploymentRecordsByMentorIdAsync(int mentorId)
        {
            return await _context.EmploymentRecords
                .AsNoTracking()
                .Include(er => er.Company)
                .Include(er => er.Position)
                .Where(er => er.MentorId == mentorId)
                .OrderByDescending(er => er.StartDate)
                .ToListAsync();
        }

        public async Task<EmploymentRecord?> GetEmploymentRecordByIdAsync(int id)
        {
            return await _context.EmploymentRecords
                .Include(er => er.Company)
                .Include(er => er.Position)
                .FirstOrDefaultAsync(er => er.EmploymentRId == id);
        }

        public async Task<bool> CreateEmploymentRecordAsync(EmploymentRecord record)
        {
            try
            {
                _context.EmploymentRecords.Add(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEmploymentRecordAsync(EmploymentRecord record)
        {
            try
            {
                _context.EmploymentRecords.Update(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEmploymentRecordAsync(int id)
        {
            try
            {
                var record = await _context.EmploymentRecords.FindAsync(id);
                if (record is null) return false;

                _context.EmploymentRecords.Remove(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Company?> GetOrCreateCompanyAsync(string name)
        {
            var existing = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyName.ToLower() == name.ToLower());

            if (existing != null) return existing;

            var company = new Company { CompanyName = name };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Position?> GetOrCreatePositionAsync(string name)
        {
            var existing = await _context.Positions
                .FirstOrDefaultAsync(p => p.Position1.ToLower() == name.ToLower());

            if (existing != null) return existing;

            var position = new Position { Position1 = name };
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }
    }
}