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

    public class GraduationRecordRepository : IGraduationRecordRepository
    {
        private readonly AppDbContext _context;
        public GraduationRecordRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<(List<GraduationRecord> Items, int TotalCount)> GetGraduationRecordsPagedAsync(int page, int pageSize, string? degreeCode = null, short? graduationYear = null)
        {
            var query = _context.GraduationRecords.Include(gr => gr.Degree).Include(gr => gr.Student).AsQueryable();

            if (!string.IsNullOrEmpty(degreeCode) ) 
            {
                query = query.Where(gr => gr.Degree.DegreeCode.Contains(degreeCode.Trim()));
            }
            if(graduationYear.HasValue)
            {
                query = query.Where(gr => gr.GraduationYear == graduationYear.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                        .OrderByDescending(gr => gr.GraduationYear).ThenBy(g => g.Grn).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<List<short>> GetDistinctGraduationYearsAsync()
        {
            return await _context.GraduationRecords
                .AsNoTracking()
                .Select(gr => gr.GraduationYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<bool> DeleteGraduationRecordAsync(int grecordId)
        {
            try
            {
                var record = await _context.GraduationRecords.FindAsync(grecordId);
                if (record is null) return false;

                _context.GraduationRecords.Remove(record);

                if (record.StudentId.HasValue)
                {
                    var student = await _context.Students
                        .Include(s => s.GraduationRecords)
                        .FirstOrDefaultAsync(s => s.StudentId == record.StudentId.Value);

                    if (student != null && !student.GraduationRecords.Any(r => r.GrecordId != grecordId))
                    {
                        student.GraduationStatus = "Undergraduate";
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<GraduationRecord?> GetGraduationRecordByIdAsync(int grecordId)
        {
            return await _context.GraduationRecords
                .Include(gr => gr.Degree)
                .Include(gr => gr.Student)
                .FirstOrDefaultAsync(gr => gr.GrecordId == grecordId);
        }

        public async Task<bool> UpdateGraduationRecordAsync(GraduationRecord record)
        {
            try
            {
                var existing = await _context.GraduationRecords.FindAsync(record.GrecordId);
                if (existing is null) return false;

                existing.OfficialName = record.OfficialName;
                existing.Grn = record.Grn;
                existing.GraduationYear = record.GraduationYear;
                existing.DegreeId = record.DegreeId;
                existing.AccStatus = record.AccStatus;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Degree>> GetDegreesAsync()
        {
            return await _context.Degrees.AsNoTracking().ToListAsync();
        }
    }
}
