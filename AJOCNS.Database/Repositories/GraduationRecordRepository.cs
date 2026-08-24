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
            var query = _context.GraduationRecords.Include(gr => gr.Degree).AsQueryable();

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
    }
}
