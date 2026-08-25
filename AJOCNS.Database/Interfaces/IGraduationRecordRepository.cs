using AJOCNS.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Database.Interfaces
{
    public interface IGraduationRecordRepository
    {
        Task<(List<GraduationRecord> Items, int TotalCount)> GetGraduationRecordsPagedAsync(int page, int pageSize, string? degreeCode = null, short? graduationYear = null);
        Task<List<short>> GetDistinctGraduationYearsAsync();
        Task<bool> DeleteGraduationRecordAsync(int grecordId);
        Task<GraduationRecord?> GetGraduationRecordByIdAsync(int grecordId);
        Task<bool> UpdateGraduationRecordAsync(GraduationRecord record);
        Task<List<Degree>> GetDegreesAsync();
    }
}
