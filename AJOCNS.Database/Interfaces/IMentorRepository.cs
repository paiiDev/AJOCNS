using AJOCNS.Database.Entities;

namespace AJOCNS.Database.Interfaces
{
    public interface IMentorRepository
    {
        Task<List<Mentor>> GetAllMentorsAsync();

        Task<Mentor?> GetMentorByUserIdAsync(int userId);
        Task<Mentor?> GetMentorByIdAsync(int mentorId);
        Task<bool> UpdateMentorAsync(Mentor mentor);
        Task<List<EmploymentRecord>> GetEmploymentRecordsByMentorIdAsync(int mentorId);
        Task<EmploymentRecord?> GetEmploymentRecordByIdAsync(int id);
        Task<bool> CreateEmploymentRecordAsync(EmploymentRecord record);
        Task<bool> UpdateEmploymentRecordAsync(EmploymentRecord record);
        Task<bool> DeleteEmploymentRecordAsync(int id);
        Task<Company?> GetOrCreateCompanyAsync(string name);
        Task<Position?> GetOrCreatePositionAsync(string name);
    }
}