using AJOCNS.Shared.DTOs.Mentor;
using AJOCNS.Shared.Common;

namespace AJOCNS.Domain.Interfaces
{
    public interface IMentorService
    {
        Task<Result<MentorProfileDto>> GetMentorProfileAsync(int userId);
        Task<Result<List<MentorProfileDto>>> GetAllMentorsAsync();
        Task<Result<MentorProfileDto>> UpdateMentorProfileAsync(int userId, string? expertise);
        Task<Result<List<EmploymentRecordDto>>> GetEmploymentRecordsAsync(int mentorId);
        Task<Result<EmploymentRecordDto>> CreateEmploymentRecordAsync(int mentorId, CreateEmploymentRecordDto dto);
        Task<Result<EmploymentRecordDto>> UpdateEmploymentRecordAsync(int mentorId, UpdateEmploymentRecordDto dto);
        Task<Result<bool>> DeleteEmploymentRecordAsync(int mentorId, int recordId);
    }
}