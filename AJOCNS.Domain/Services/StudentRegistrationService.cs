using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.StudentRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            string body = $"Hello {studentRegistrationDto.Name}, your Student Registration Number (SRN) is {newSRN}. Your password is: {rawPassword}. Use this password to log in at AJOCNS.";
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
    }
}
