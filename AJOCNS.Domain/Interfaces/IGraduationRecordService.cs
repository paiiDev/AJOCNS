using AJOCNS.Shared.DTOs.GraduationRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Interfaces
{
    public interface IGraduationRecordService
    {
        Task<(List<GraduationRecordDto> Items, int TotalCount)> GetGraduationRecordsPagedAsync(int page, int pageSize, string? degreeCode = null, short? graduationYear = null);
    }
}
