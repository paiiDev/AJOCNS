using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Jobs;

namespace AJOCNS.Domain.Interfaces
{
    public interface IJobService
    {
        Task<Result<bool>> CreateJobPostAsync(CreateJobPostDto dto, int postedByUserId, bool autoApprove, DateTime closingDateUtc);
        Task<Result<UpdateJobPostDto>> GetJobPostForEditAsync(int id);
        Task<Result<bool>> UpdateJobPostAsync(UpdateJobPostDto dto, int currentUserId, bool isAdmin, DateTime closingDateUtc);
        Task<Result<bool>> DeleteJobPostAsync(int id, int currentUserId, bool isAdmin);
        Task<Result<List<JobPostDto>>> GetAllJobPostsAsync();
        Task<Result<PagedJobPostDto>> GetJobPostsPagedAsync(int page, int pageSize, string? jobType = null, string? status = null);
        Task<Result<bool>> ApproveJobPostAsync(int jobPostId);
        Task<Result<bool>> RejectJobPostAsync(int jobPostId);
        Task<Result<List<JobStatusDto>>> GetJobStatusesAsync();
    }
}