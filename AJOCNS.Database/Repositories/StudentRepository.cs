using AJOCNS.Database.Context;
using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Database.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;
        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email);
        }

        public async Task<string?> GetLastSRNAsync()
        {
            var lastStudent = await _context.Students.OrderByDescending(s => s.Srn).FirstOrDefaultAsync();
            return lastStudent?.Srn;
        }

        public async Task<bool> SaveStudentAsync(User newUser)
        {
           using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
           return await _context.Students.AsNoTracking().Include(m =>  m.Major).ToListAsync();
        }

        public async Task<(List<Student> Items, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, int? majorId, int? acyId)
        {
            var query = _context.Students.AsNoTracking()
                .Include(s => s.Major)
                .Include(s => s.Enrollments).ThenInclude(e => e.Acy)
                .AsQueryable();

            if (majorId.HasValue)
            {
                query = query.Where(s => s.MajorId == majorId.Value);
            }

            if (acyId.HasValue)
            {
                query = query.Where(s => s.Enrollments.Any(e => e.AcyId == acyId.Value));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.Srn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> BulkUpdateMajorsAsync(Dictionary<int, int> studentMajorPairs)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var studentIds = studentMajorPairs.Keys.ToList();
                var students = await _context.Students
                    .Where(s => studentIds.Contains(s.StudentId))
                    .ToListAsync();

                if (students.Count == 0) return false;

                foreach (var student in students)
                {
                    if (studentMajorPairs.TryGetValue(student.StudentId, out int majorId))
                    {
                        student.MajorId = majorId;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> BulkUpdateGraduationsAsync(Dictionary<int, string> studentStatusPairs)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var studentIds = studentStatusPairs.Keys.ToList();
                var students = await _context.Students
                    .Include(s => s.GraduationRecords)
                    .Where(s => studentIds.Contains(s.StudentId))
                    .ToListAsync();

                if (students.Count == 0) return false;

                short graduationYear = (short)DateTime.Now.Year;
                int? defaultDegreeId = await _context.Degrees
                    .OrderBy(d => d.DegreeId)
                    .Select(d => (int?)d.DegreeId)
                    .FirstOrDefaultAsync();

                foreach (var student in students)
                {
                    if (!studentStatusPairs.TryGetValue(student.StudentId, out string? status))
                        continue;

                    student.GraduationStatus = status;

                    if (status == "Graduated"
                        && !student.GraduationRecords.Any()
                        && defaultDegreeId.HasValue)
                    {
                        _context.GraduationRecords.Add(new GraduationRecord
                        {
                            OfficialName = student.Name,
                            Grn = GenerateNewGRN(),
                            GraduationYear = graduationYear,
                            DegreeId = defaultDegreeId.Value,
                            AccStatus = "Active",
                            StudentId = student.StudentId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        private static string GenerateNewGRN()
        {
            string year = DateTime.Now.ToString("yyyy");
            string random = Random.Shared.Next(10000, 99999).ToString();
            return $"PUPL-{year}-{random}";
        }

        public async Task<List<Major>> GetAllMajorsAsync()
        {
            return await _context.Majors.AsNoTracking().ToListAsync();
        }

        public async Task<List<Major>> GetFoundationMajorsAsync()
        {
            return await _context.Majors
                .Where(m => m.IsFoundation == true)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<AcademicYear>> GetAcademicYearsAsync()
        {
            return await _context.AcademicYears.AsNoTracking().ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            try
            {
                var existing = await _context.Students.FindAsync(student.StudentId);
                if (existing is null) return false;

                existing.Name = student.Name;
                existing.Phone = student.Phone;
                existing.FatherName = student.FatherName;
                existing.Address = student.Address;
                existing.MajorId = student.MajorId;
                existing.GraduationStatus = student.GraduationStatus;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student is null) return false;

                student.User.Status = "Inactive";
                student.User.IsDeleted = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Degree>> GetDegreesAsync()
        {
            return await _context.Degrees.AsNoTracking().ToListAsync();
        }

    }
}