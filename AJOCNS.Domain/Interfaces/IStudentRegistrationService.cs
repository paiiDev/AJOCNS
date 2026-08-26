using AJOCNS.Database.Entities;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Dashboard;
using AJOCNS.Shared.DTOs.StudentRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Interfaces
{
    public interface IStudentRegistrationService
    {
        Task<Result<bool>> RegisterStudentAsync(StudentRegistrationDto studentRegistrationDto);
        Task<Result<List<StudentDto>>> GetAllStudentsAsync();
        Task<Result<int>> GetActiveStudentCountAsync();
        Task<Result<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<Result<PagedStudentDto>> GetStudentsPagedAsync(int page, int pageSize, int? majorId, int? acyId, string? excludeGraduationStatus = null);
        Task<Result<bool>> BulkUpdateMajorsAsync(List<BulkMajorUpdateItemDto> updates);
        Task<Result<bool>> BulkUpdateGraduationsAsync(BulkGraduationUpdateRequestDto request);
        Task<Result<List<MajorDto>>> GetMajorsAsync();
        Task<Result<List<MajorDto>>> GetFoundationMajorsAsync();
        Task<Result<List<AcademicYearDto>>> GetAcademicYearsAsync();

        Task<Result<EditStudentDto>> GetStudentByIdAsync(int studentId);
        Task<Result<bool>> UpdateStudentAsync(EditStudentDto dto);
        Task<Result<bool>> DeleteStudentAsync(int studentId);
    }
}
