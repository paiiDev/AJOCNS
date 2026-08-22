using AJOCNS.Shared.Common;
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
        Task<Result<List<MajorDto>>> GetMajorsAsync();
        Task<Result<EditStudentDto>> GetStudentByIdAsync(int studentId);
        Task<Result<bool>> UpdateStudentAsync(EditStudentDto dto);
        Task<Result<bool>> DeleteStudentAsync(int studentId);
    }
}
