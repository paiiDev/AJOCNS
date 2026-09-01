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
           return await _context.Students.AsNoTracking().Include(m => m.Major).Include(s => s.GraduationRecords).ToListAsync();
        }

        public async Task<int> CountActiveStudentsAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .CountAsync(s => s.User.Status == "Active");
        }

        public async Task<(int Total, int Graduated, int Undergraduate, int Dropout)> GetStudentStatusCountsAsync()
        {
            var groups = await _context.Students
                .AsNoTracking()
                .GroupBy(s => s.GraduationStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int graduated = groups.Where(g => g.Status == "Graduated").Sum(g => g.Count);
            int dropout = groups.Where(g => g.Status == "Dropout").Sum(g => g.Count);
            int undergraduate = groups.Where(g => g.Status != "Graduated" && g.Status != "Dropout").Sum(g => g.Count);
            int total = groups.Sum(g => g.Count);

            return (total, graduated, undergraduate, dropout);
        }

        public async Task<int> CountActiveMentorsAsync()
        {
            return await _context.Mentors
                .AsNoTracking()
                .CountAsync(m => m.User.Status == "Active");
        }

        public async Task<int> CountPendingEventRegistrationsAsync()
        {
            return await _context.EventRegistrations
                .AsNoTracking()
                .CountAsync(er => er.Status.ToLower().Contains("pend"));
        }

        public async Task<int> CountCareerEventsAsync()
        {
            return await _context.Events.Where(e  => e.Status == "Completed")
                .AsNoTracking()
                .CountAsync();
        }

        public async Task<(List<Student> Items, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, int? majorId, int? acyId, string? excludeGraduationStatus)
        {
            var query = _context.Students.AsNoTracking()
                .Include(s => s.Major)
                .Include(s => s.Enrollments).ThenInclude(e => e.Acy)
                .Include(s => s.GraduationRecords)
                .AsQueryable();

            if (majorId.HasValue)
            {
                query = query.Where(s => s.MajorId == majorId.Value);
            }

            if (acyId.HasValue)
            {
                query = query.Where(s => s.Enrollments.Any(e => e.AcyId == acyId.Value));
            }

            if (!string.IsNullOrEmpty(excludeGraduationStatus))
            {
                query = query.Where(s => (s.GraduationStatus ?? "Undergraduate") != excludeGraduationStatus);
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

        public async Task<bool> BulkUpdateGraduationsAsync(Dictionary<int, string> studentStatusPairs, short graduationYear)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var studentIds = studentStatusPairs.Keys.ToList();
                var students = await _context.Students
                    .Include(s => s.GraduationRecords)
                    .Include(s => s.Major)
                    .ThenInclude(d => d.Degree)
                    .Where(s => studentIds.Contains(s.StudentId))
                    .ToListAsync();

                if (students.Count == 0) return false;

                string nextGrn = await GenerateNextGRN(graduationYear);
                int grnSequence = int.Parse(nextGrn.Substring(nextGrn.LastIndexOf('-') + 1));

                int? defaultDegreeId = await _context.Degrees
                    .OrderBy(d => d.DegreeId)
                    .Select(d => (int?)d.DegreeId)
                    .FirstOrDefaultAsync();

                foreach (var student in students)
                {
                    if (!studentStatusPairs.TryGetValue(student.StudentId, out string? status))
                        continue;

                    student.GraduationStatus = status;

                    if (status == "Graduated")
                    {
                        if (!student.GraduationRecords.Any())
                        {
                            int degreeId = student.Major?.DegreeId ?? defaultDegreeId ?? 0;
                            if (degreeId == 0) continue;

                            _context.GraduationRecords.Add(new GraduationRecord
                            {
                                OfficialName = student.Name,
                                Grn = $"PUPL-{graduationYear}-{grnSequence:D5}",
                                GraduationYear = graduationYear,
                                DegreeId = degreeId,
                                AccStatus = "Active",
                                StudentId = student.StudentId
                            });
                            grnSequence++;
                        }
                    }
                    else
                    {
                        var recordsToRemove = _context.GraduationRecords
                            .Where(gr => gr.StudentId == student.StudentId);
                        _context.GraduationRecords.RemoveRange(recordsToRemove);
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

        private async Task<string> GenerateNextGRN(short graduationYear)
        {
            string prefix = $"PUPL-{graduationYear}-";

            var existingGrns = await _context.GraduationRecords
                .Where(gr => gr.Grn.StartsWith(prefix))
                .Select(gr => gr.Grn)
                .ToListAsync();

            int maxNumber = 0;
            foreach (var grn in existingGrns)
            {
                if (int.TryParse(grn.Substring(prefix.Length), out int number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return $"{prefix}{(maxNumber + 1):D5}";
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
                .Include(s => s.Enrollments)
                .Include(s => s.GraduationRecords)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<Student?> GetStudentByUserIdAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.Major)
                .Include(s => s.User)
                .Include(s => s.Enrollments).ThenInclude(e => e.Acy)
                .Include(s => s.GraduationRecords)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<bool> UpdateStudentEnrollmentAcyAsync(int studentId, int acyId)
        {
            try
            {
                var enrollment = await _context.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .OrderByDescending(e => e.ErId)
                    .FirstOrDefaultAsync();

                if (enrollment is null)
                {
                    bool studentExists = await _context.Students.AnyAsync(s => s.StudentId == studentId);
                    if (!studentExists) return false;

                    _context.Enrollments.Add(new Enrollment
                    {
                        StudentId = studentId,
                        AcyId = acyId,
                        Status = "Enrolled"
                    });
                }
                else
                {
                    enrollment.AcyId = acyId;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
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