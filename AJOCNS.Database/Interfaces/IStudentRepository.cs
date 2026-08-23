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
        Task<bool> SaveStudentAsync(User newUser, Student newStudent);
        Task<List<Student>> GetAllStudentsAsync();
        Task<(List<Student> Items, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, int? majorId);
        Task<bool> BulkUpdateMajorsAsync(Dictionary<int, int> studentMajorPairs);
        Task<List<Major>> GetAllMajorsAsync();
        Task<List<Major>> GetFoundationMajorsAsync();
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<bool> UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int studentId);
        Task<List<Degree>> GetDegreesAsync();
        Task<bool> AddGraduationRecordAsync(GraduationRecord record, int studentId);
    }
}
