using AJOCNS.Database.Entities;

namespace AJOCNS.Database.Interfaces
{
    public interface IJobRepository
    {
        Task<bool> CreateJobPostAsync(JobPost jobPost);
        Task<bool> UpdateJobPostAsync(JobPost jobPost);
        Task<bool> DeleteJobPostAsync(int id);
        Task<JobPost?> GetJobPostById(int? id);
        Task<List<JobPost>> GetAllJobPostsAsync();
        Task<List<JobPost>> GetOpenJobPostsAsync();
        Task<(List<JobPost> Items, int TotalCount)> GetJobPostsPagedAsync(int page, int pageSize, string? jobType = null, string? status = null);
        Task<(List<JobPost> Items, int TotalCount)> GetJobPostsPagedForUserAsync(int userId, int page, int pageSize, string? jobType = null, string? status = null);
        Task<bool> UpdateJobStatusAsync(int jobPostId, string status);
        Task<List<JobPost>> GetJobStatusesAsync();
        Task<List<JobPost>> GetPendingJobPostsAsync();
        Task<int> CountPendingJobPostsAsync();
    }
}