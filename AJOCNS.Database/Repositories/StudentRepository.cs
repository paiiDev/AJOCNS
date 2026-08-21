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
            return await _context.Users.AnyAsync(u => u.Email == email);
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

        public async Task<List<Major>> GetAllMajorsAsync()
        {
            return await _context.Majors.AsNoTracking().ToListAsync();
        } 
    }
}