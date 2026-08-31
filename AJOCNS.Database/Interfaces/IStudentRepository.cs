using AJOCNS.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Database.Interfaces
{
    public interface IStudentRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<string?> GetLastSRNAsync(); 
        Task<bool> SaveStudentAsync(User newUser);
        Task<List<Student>> GetAllStudentsAsync();
        Task<int> CountActiveStudentsAsync();
        Task<int> CountActiveMentorsAsync();
        Task<int> CountPendingEventRegistrationsAsync();
        Task<int> CountCareerEventsAsync();
        Task<(int Total, int Graduated, int Undergraduate, int Dropout)> GetStudentStatusCountsAsync();
        Task<(List<Student> Items, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, int? majorId, int? acyId, string? excludeGraduationStatus = null);
        Task<bool> BulkUpdateMajorsAsync(Dictionary<int, int> studentMajorPairs);
        Task<bool> BulkUpdateGraduationsAsync(Dictionary<int, string> studentStatusPairs, short graduationYear);
        Task<List<Major>> GetAllMajorsAsync();
        Task<List<Major>> GetFoundationMajorsAsync();
        Task<List<AcademicYear>> GetAcademicYearsAsync();
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<Student?> GetStudentByUserIdAsync(int userId);
        Task<bool> UpdateStudentAsync(Student student);
        Task<bool> UpdateStudentEnrollmentAcyAsync(int studentId, int acyId);
        Task<bool> DeleteStudentAsync(int studentId);
        Task<List<Degree>> GetDegreesAsync();
    }
}
