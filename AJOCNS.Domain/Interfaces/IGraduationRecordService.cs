using AJOCNS.Shared.Common;
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
        Task<Result<PagedGraduationRecordDto>> GetGraduationRecordsPagedAsync(int page, int pageSize, string? degreeCode = null, short? graduationYear = null);
        Task<Result<List<short>>> GetGraduationYearsAsync();
        Task<Result<bool>> DeleteGraduationRecordAsync(int grecordId);
        Task<Result<EditGraduationRecordDto>> GetGraduationRecordByIdAsync(int grecordId);
        Task<Result<bool>> UpdateGraduationRecordAsync(EditGraduationRecordDto dto);
        Task<Result<List<DegreeOptionDto>>> GetDegreesAsync();
    }
}
