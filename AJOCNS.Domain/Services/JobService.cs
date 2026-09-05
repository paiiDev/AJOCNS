using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Jobs;

namespace AJOCNS.Domain.Services
{
    public class JobService : IJobService
    {
        private static readonly TimeZoneInfo MyanmarTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
        private readonly IJobRepository _jobRepo;

        public JobService(IJobRepository jobRepo)
        {
            _jobRepo = jobRepo;
        }

        private static JobPostDto ToJobPostDto(JobPost j) => new JobPostDto
        {
            Id = j.JobPostId,
            PostedByUserId = j.PostedByUserId,
            Title = j.Title,
            CompanyName = j.CompanyName,
            Description = j.Description,
            Requirements = j.Requirements,
            JobType = j.JobType,
            Location = j.Location,
            SalaryRange = j.SalaryRange,
            PostedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(j.PostedDate, DateTimeKind.Utc), MyanmarTimeZone),
            ClosingDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(j.ClosingDate, DateTimeKind.Utc), MyanmarTimeZone),
            Status = j.Status,
            PostedByName = j.PostedByUser?.Email ?? "Unknown"
        };

        public async Task<Result<bool>> CreateJobPostAsync(CreateJobPostDto dto, int postedByUserId, bool autoApprove, DateTime closingDateUtc)
        {
            if (dto is null)
                return Result<bool>.Failure("Invalid job post data.");

            if (closingDateUtc < DateTime.UtcNow.AddHours(-1))
                return Result<bool>.Failure("Closing date cannot be in the past.");

            var newJobPost = new JobPost
            {
                PostedByUserId = postedByUserId,
                Title = dto.Title.Trim(),
                CompanyName = dto.CompanyName.Trim(),
                Description = dto.Description.Trim(),
                Requirements = dto.Requirements,
                JobType = dto.JobType,
                Location = dto.Location,
                SalaryRange = dto.SalaryRange,
                PostedDate = DateTime.UtcNow,
                ClosingDate = closingDateUtc,
                Status = autoApprove ? "Open" : "Pending",
                IsDeleted = false
            };

            bool saved = await _jobRepo.CreateJobPostAsync(newJobPost);
            if (!saved)
                return Result<bool>.Failure("Failed to create job post.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<UpdateJobPostDto>> GetJobPostForEditAsync(int id)
        {
            var jobPost = await _jobRepo.GetJobPostById(id);
            if (jobPost == null)
                return Result<UpdateJobPostDto>.Failure("Job post not found.");

            var dto = new UpdateJobPostDto
            {
                Id = jobPost.JobPostId,
                Title = jobPost.Title,
                CompanyName = jobPost.CompanyName,
                Description = jobPost.Description,
                Requirements = jobPost.Requirements,
                JobType = jobPost.JobType,
                Location = jobPost.Location,
                SalaryRange = jobPost.SalaryRange,
                ClosingDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(jobPost.ClosingDate, DateTimeKind.Utc), MyanmarTimeZone)
            };

            return Result<UpdateJobPostDto>.Success(dto);
        }

        public async Task<Result<bool>> UpdateJobPostAsync(UpdateJobPostDto dto, int currentUserId, bool isAdmin, DateTime closingDateUtc)
        {
            if (dto is null)
                return Result<bool>.Failure("Invalid job post data.");

            var existing = await _jobRepo.GetJobPostById(dto.Id);
            if (existing == null)
                return Result<bool>.Failure("Job post not found.");

            if (!isAdmin && existing.PostedByUserId != currentUserId)
                return Result<bool>.Failure("You do not have permission to edit this job post.");

            if (!isAdmin && DateTime.UtcNow > closingDateUtc)
                return Result<bool>.Failure("Closing date cannot be in the past.");

            existing.Title = dto.Title.Trim();
            existing.CompanyName = dto.CompanyName.Trim();
            existing.Description = dto.Description.Trim();
            existing.Requirements = dto.Requirements;
            existing.JobType = dto.JobType;
            existing.Location = dto.Location;
            existing.SalaryRange = dto.SalaryRange;
            existing.ClosingDate = closingDateUtc;

            bool updated = await _jobRepo.UpdateJobPostAsync(existing);
            if (!updated)
                return Result<bool>.Failure("Failed to update job post.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteJobPostAsync(int id, int currentUserId, bool isAdmin)
        {
            var existing = await _jobRepo.GetJobPostById(id);
            if (existing == null)
                return Result<bool>.Failure("Job post not found.");

            if (!isAdmin && existing.PostedByUserId != currentUserId)
                return Result<bool>.Failure("You do not have permission to delete this job post.");

            bool deleted = await _jobRepo.DeleteJobPostAsync(id);
            if (!deleted)
                return Result<bool>.Failure("Failed to delete job post.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<JobPostDto>>> GetAllJobPostsAsync()
        {
            var jobPosts = await _jobRepo.GetAllJobPostsAsync();
            var jobPostDtos = (jobPosts ?? new List<JobPost>())
                .Select(j => BuildJobPostDto(j))
                .ToList();

            return Result<List<JobPostDto>>.Success(jobPostDtos);
        }

        public async Task<Result<PagedJobPostDto>> GetJobPostsPagedAsync(int page, int pageSize, string? jobType = null, string? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _jobRepo.GetJobPostsPagedAsync(page, pageSize, jobType, status);

            var paged = new PagedJobPostDto
            {
                Jobs = (items ?? new List<JobPost>()).Select(ToJobPostDto).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Result<PagedJobPostDto>.Success(paged);
        }

        public async Task<Result<bool>> ApproveJobPostAsync(int jobPostId)
        {
            bool updated = await _jobRepo.UpdateJobStatusAsync(jobPostId, "Open");
            if (!updated)
                return Result<bool>.Failure("Failed to approve job post.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectJobPostAsync(int jobPostId)
        {
            bool updated = await _jobRepo.UpdateJobStatusAsync(jobPostId, "Rejected");
            if (!updated)
                return Result<bool>.Failure("Failed to reject job post.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<JobStatusDto>>> GetJobStatusesAsync()
        {
            var statuses = await _jobRepo.GetJobStatusesAsync();
            if (statuses == null || !statuses.Any())
            {
                return Result<List<JobStatusDto>>.Failure("No job statuses found");
            }

            var statusDtos = statuses.Select(s => new JobStatusDto
            {
                Status = s.Status
            }).ToList();

            return Result<List<JobStatusDto>>.Success(statusDtos);
        }

        private JobPostDto BuildJobPostDto(JobPost j)
        {
            var dto = ToJobPostDto(j);
            dto.PostedByName = GetCreatorName(j.PostedByUser);
            return dto;
        }

        private string GetCreatorName(User? user)
        {
            if (user == null) return "Unknown";

            return user.Role.ToLower() switch
            {
                "admin" => user.Admin?.Name ?? user.Email,
                "mentor" => user.Mentor?.Name ?? user.Email,
                "externalpartner" => user.ExternalPartner?.Name ?? user.Email,
                _ => user.Email
            };
        }
    }
}