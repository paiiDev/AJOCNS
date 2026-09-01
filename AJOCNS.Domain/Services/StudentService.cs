using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Student;
using AJOCNS.Shared.DTOs.StudentDashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepo;
        public StudentService(IStudentRepository studentRepo)
        {
            _studentRepo = studentRepo;
        }
        public async Task<Result<StudentDashboardDto>> GetStudentDashboardAsync(int userId)
        {
            var student = await _studentRepo.GetStudentByUserIdAsync(userId);
            if (student is null)
                return Result<StudentDashboardDto>.Failure("Student profile not found.");

            var latestEnrollment = student.Enrollments.OrderByDescending(e => e.ErId).FirstOrDefault();

            var dto = new StudentDashboardDto
            {
                StudentId = student.StudentId,
                Srn = student.Srn,
                Name = student.Name,
                Email = student.User?.Email ?? "-",
                Phone = student.Phone,
                Major = student.Major?.MajorName ?? "-",
                AcademicYear = latestEnrollment?.Acy?.AcademicYear1,
                GraduationStatus = student.GraduationStatus ?? "Undergraduate",
                IsGraduated = student.GraduationRecords != null && student.GraduationRecords.Any(),
                GraduationYear = student.GraduationRecords.Select(r => (short?)r.GraduationYear).FirstOrDefault()
            };

            return Result<StudentDashboardDto>.Success(dto);
        }

        public async Task<Result<bool>> SetupStudentFirstLoginAsync(int userId, StudentFirstLoginSetupDto dto)
        {
            var student = await _studentRepo.GetStudentByUserIdAsync(userId);
            if (student is null)
                return Result<bool>.Failure("Student profile not found.");

            var newPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            student.Phone = dto.PhoneNumber;
            student.FatherName = dto.FatherName;
            student.Address = dto.Address;
            student.User.PasswordHash = newPassword; 
            student.User.IsFirstLogin = false;
            var updateResult = await _studentRepo.UpdateStudentAsync(student);
            if (!updateResult)
                return Result<bool>.Failure("Failed to update student profile.");
            return Result<bool>.Success(true);
        }

    }
}
