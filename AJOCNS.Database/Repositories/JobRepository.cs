using AJOCNS.Database.Context;
using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AJOCNS.Database.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _context;

        public JobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateJobPostAsync(JobPost jobPost)
        {
            try
            {
                _context.JobPosts.Add(jobPost);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateJobPostAsync(JobPost jobPost)
        {
            try
            {
                _context.JobPosts.Update(jobPost);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteJobPostAsync(int id)
        {
            try
            {
                var jobPost = await _context.JobPosts.FindAsync(id);
                if (jobPost is null) return false;

                jobPost.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<JobPost?> GetJobPostById(int? id)
        {
            return await _context.JobPosts
                .Include(j => j.PostedByUser).ThenInclude(u => u.Admin)
                .Include(j => j.PostedByUser).ThenInclude(u => u.Mentor)
                .Include(j => j.PostedByUser).ThenInclude(u => u.ExternalPartner)
                .FirstOrDefaultAsync(x => x.JobPostId == id);
        }

        public async Task<List<JobPost>> GetAllJobPostsAsync()
        {
            return await _context.JobPosts
                .AsNoTracking()
                .Include(j => j.PostedByUser)
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();
        }

        public async Task<List<JobPost>> GetJobStatusesAsync()
        {
            return await _context.JobPosts
                .AsNoTracking()
                .Select(j => new JobPost
                {
                    Status = j.Status
                }).Distinct()
                .ToListAsync();
        }

        public async Task<(List<JobPost> Items, int TotalCount)> GetJobPostsPagedAsync(int page, int pageSize, string? jobType = null, string? status = null)
        {
            var query = _context.JobPosts
                .AsNoTracking()
                .Include(j => j.PostedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(j => j.JobType == jobType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(j => j.Status == status);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(j => j.Status == "Open")
                .ThenByDescending(j => j.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> UpdateJobStatusAsync(int jobPostId, string status)
        {
            try
            {
                var jobPost = await _context.JobPosts.FindAsync(jobPostId);
                if (jobPost is null) return false;

                jobPost.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> CountPendingJobPostsAsync()
        {
            return await _context.JobPosts
                .AsNoTracking()
                .CountAsync(j => j.Status.ToLower().Contains("pend"));
        }
    }
}