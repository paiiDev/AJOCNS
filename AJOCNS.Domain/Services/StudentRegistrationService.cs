using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.StudentRegistration;

namespace AJOCNS.Domain.Services
{
    public class StudentRegistrationService : IStudentRegistrationService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IEmailService _emailService;
        public StudentRegistrationService(IStudentRepository studentRepo, IEmailService emailService)
        {
            _studentRepo = studentRepo;
            _emailService = emailService;
        }

        public async Task<Result<bool>> RegisterStudentAsync(StudentRegistrationDto studentRegistrationDto)
        {
            var existingStudent = await _studentRepo.EmailExistsAsync(studentRegistrationDto.Email);
            if(existingStudent)
            {
                return Result<bool>.Failure("Email already exists");
            }

            string lastSRN = await _studentRepo.GetLastSRNAsync();
            string newSRN = GenerateNewSRN(lastSRN);

            string rawPassword = $"PUPL@{new Random().Next(100000, 999999)}";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            var newUser = new User
            {
                Email = studentRegistrationDto.Email,
                PasswordHash = hashedPassword,
                Role = "Student",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                IsFirstLogin = true,
                IsDeleted = false,
            };

            var newStudent = new Student
            {
                Name = studentRegistrationDto.Name,
                MajorId = studentRegistrationDto.Major_ID,
                GrecordId = null,
                GraduationStatus = studentRegistrationDto.GraduationStatus,
                Srn = newSRN,
            };

            bool isSaved = await _studentRepo.SaveStudentAsync(newUser, newStudent);
            if(!isSaved)
            {
                return Result<bool>.Failure("Failed to save student");
            }

            string body = $@"
<div style='font-family: Arial, sans-serif; background-color: #f4f5f7; padding: 40px 20px; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='background-color: #0056b3; color: #ffffff; padding: 20px; text-align: center;'>
            <h2 style='margin: 0; font-size: 24px;'>Welcome to AJOCNS</h2>
        </div>

        <!-- Body -->
        <div style='padding: 30px; line-height: 1.6; font-size: 16px;'>
            <p>Hello! <strong>{studentRegistrationDto.Name}</strong>,</p>
            <p>Your student account has been successfully created. Below are your official login credentials:</p>
            
            <!-- Credentials Box -->
            <div style='background-col or: #f8f9fa; padding: 20px; border-left: 4px solid #0056b3; margin: 25px 0; border-radius: 4px;'>
                <p style='margin: 0 0 10px 0;'><strong>Student Registration Number (SRN):</strong> <span style='color: #0056b3;'>{newSRN}</span></p>
                <p style='margin: 0;'><strong>Temporary Password:</strong> <span>{rawPassword}</span></p>
            </div>

            <p>Please use these credentials to log in to the student portal. As a security measure, you will be asked to complete your profile upon your first login.</p>
            
            <!-- Login Button -->
            <div style='text-align: center; margin-top: 35px;'>
                <a href='https://your-domain.com/login' style='background-color: #0056b3; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Log In to Portal</a>
            </div>
        </div>

        <!-- Footer -->
        <div style='background-color: #f4f5f7; padding: 15px; text-align: center; font-size: 12px; color: #777777; border-top: 1px solid #e9ecef;'>
            <p style='margin: 0;'>This is an automated message. Please do not reply to this email.</p>
            <p style='margin: 5px 0 0 0;'>&copy; {DateTime.Now.Year} AJOCNS. All rights reserved.</p>
        </div>

    </div>
</div>";
            await _emailService.SendEmailAsync(studentRegistrationDto.Email,"Welcome to AJOCNS", body);

            return Result<bool>.Success(true);
        }

        private string GenerateNewSRN(string lastSRN)
        {
            if (string.IsNullOrEmpty(lastSRN))
            {
                return "PUPL-00001"; 
            }

           
            string numberPart = lastSRN.Replace("PUPL-", "");

            if (int.TryParse(numberPart, out int lastNumber))
            {
                int nextNumber = lastNumber + 1;
                return $"PUPL-{nextNumber:D5}";
            }

            return "PUPL-00001"; 
        }

        public async Task<Result<List<StudentDto>>> GetAllStudentsAsync()
        {
            var students = await _studentRepo.GetAllStudentsAsync();
            if(students == null || !students.Any())
            {
                return Result<List<StudentDto>>.Failure("No students found");
            }
            var studentDtos = students.Select(s => new StudentDto
            {
                StudentId = s.StudentId,
                Name = s.Name,
                Phone = s.Phone,
                FatherName = s.FatherName,
                Address = s.Address,
                Major = s.Major.MajorName,
                GraduationStatus = s.GraduationStatus,
                Srn = s.Srn
            }).ToList();

            return Result<List<StudentDto>>.Success(studentDtos);
        }

        public async Task<Result<List<MajorDto>>> GetMajorsAsync()
        {
            var majors = await _studentRepo.GetAllMajorsAsync();
            if(majors == null || !majors.Any())
            {
                return Result<List<MajorDto>>.Failure("No majors found");
            }
            var majorDtos = majors.Select(m => new MajorDto
            {
                Id = m.MajorId,
                MajorName = m.MajorName
            }).ToList();
            return Result<List<MajorDto>>.Success(majorDtos);
        }

        public async Task<Result<EditStudentDto>> GetStudentByIdAsync(int studentId)
        {
            var student = await _studentRepo.GetStudentByIdAsync(studentId);
            if (student is null)
                return Result<EditStudentDto>.Failure("Student not found.");

            var dto = new EditStudentDto
            {
                StudentId = student.StudentId,
                Srn = student.Srn,
                Name = student.Name,
                Phone = student.Phone,
                FatherName = student.FatherName,
                Address = student.Address,
                MajorId = student.MajorId,
                GraduationStatus = student.GraduationStatus ?? "Undergraduate"
            };

            return Result<EditStudentDto>.Success(dto);
        }

        public async Task<Result<bool>> UpdateStudentAsync(EditStudentDto dto)
        {
            var student = await _studentRepo.GetStudentByIdAsync(dto.StudentId);
            if (student is null)
                return Result<bool>.Failure("Student not found.");

            bool wasNotGraduated = student.GraduationStatus != "Graduated";
            bool isNowGraduated = dto.GraduationStatus == "Graduated";

            student.Name = dto.Name;
            student.Phone = dto.Phone;
            student.FatherName = dto.FatherName;
            student.Address = dto.Address;
            student.MajorId = dto.MajorId;
            student.GraduationStatus = dto.GraduationStatus;

            bool updated = await _studentRepo.UpdateStudentAsync(student);
            if (!updated)
                return Result<bool>.Failure("Failed to update student.");

            if (wasNotGraduated && isNowGraduated && student.GrecordId is null)
            {
                string grn = GenerateNewGRN();
                var record = new GraduationRecord
                {
                    OfficialName = student.Name,
                    Grn = grn,
                    GraduationYear = (short)DateTime.Now.Year,
                    DegreeId = dto.MajorId,
                    AccStatus = "Pending"
                };

                bool recordCreated = await _studentRepo.AddGraduationRecordAsync(record, dto.StudentId);
                if (!recordCreated)
                    return Result<bool>.Failure("Student updated but failed to create graduation record.");
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteStudentAsync(int studentId)
        {
            bool deleted = await _studentRepo.DeleteStudentAsync(studentId);
            if (!deleted)
                return Result<bool>.Failure("Failed to delete student.");

            return Result<bool>.Success(true);
        }

        private string GenerateNewGRN()
        {
            string year = DateTime.Now.ToString("yyyy");
            string random = new Random().Next(10000, 99999).ToString();
            return $"PUPL-{year}-{random}";
        }
    }
}
