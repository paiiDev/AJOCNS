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
        Task<List<Major>> GetAllMajorsAsync();
    }
}
