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

        public async Task<bool> SaveStudentAsync(User newUser, Student newStudent)
        {
           using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                newStudent.UserId = newUser.UserId;

                _context.Students.Add(newStudent);
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

        public async Task<(List<Student> Items, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, int? majorId)
        {
            var query = _context.Students.AsNoTracking().Include(s => s.Major).AsQueryable();

            if (majorId.HasValue)
            {
                query = query.Where(s => s.MajorId == majorId.Value);
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

        public async Task<bool> AddGraduationRecordAsync(GraduationRecord record, int studentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.GraduationRecords.Add(record);
                await _context.SaveChangesAsync();

                var student = await _context.Students.FindAsync(studentId);
                if (student is not null)
                {
                    student.GrecordId = record.GrecordId;
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}