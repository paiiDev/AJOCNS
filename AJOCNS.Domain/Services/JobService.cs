using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Jobs;

namespace AJOCNS.Domain.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepo;
        private readonly IEmailService _emailService;

        public JobService(IJobRepository jobRepo, IEmailService emailService)
        {
            _jobRepo = jobRepo;
            _emailService = emailService;
        }

        public async Task<Result<List<AppliedJobDto>>> GetAppliedJobsAsync(int userId)
        {
            var applications = await _jobRepo.GetApplicationsByUserIdAsync(userId);
            return Result<List<AppliedJobDto>>.Success(applications.Select(a => new AppliedJobDto
            {
                JobPostId = a.JobPostId, Title = a.JobPost.Title, CompanyName = a.JobPost.CompanyName,
                AppliedDate = MyanmarTime.ToMyanmar(a.AppliedDate), Status = a.Status
            }).ToList());
        }

        public Task<Result<List<JobPostDto>>> GetActiveJobsAsync() => GetOpenJobsAsync();

        public async Task<Result<bool>> ApplyForJobAsync(int studentId, ApplyJobDto dto)
        {
            if (dto.Resume is null || dto.Resume.Length == 0 || !string.Equals(Path.GetExtension(dto.Resume.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                return Result<bool>.Failure("A PDF resume is required.");
            var job = await _jobRepo.GetJobPostById(dto.JobPostId);
            if (job is null || job.IsDeleted || job.ClosingDate <= DateTime.UtcNow ||
                (job.Status.ToLower() == "rejected"))
                return Result<bool>.Failure("This job is no longer accepting applications.");
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid():N}.pdf";
            await using (var stream = File.Create(Path.Combine(folder, fileName))) await dto.Resume.CopyToAsync(stream);
            var saved = await _jobRepo.CreateApplicationAsync(new JobApplication
            {
                JobPostId = dto.JobPostId, UserId = studentId, CoverLetter = dto.CoverLetter.Trim(),
                ResumeUrl = $"/uploads/resumes/{fileName}", AppliedDate = DateTime.UtcNow, Status = "Pending"
            });
            return saved ? Result<bool>.Success(true) : Result<bool>.Failure("You have already applied for this job.");
        }

        public async Task<Result<List<ApplicantListDto>>> GetApplicantsByJobIdAsync(int partnerUserId, int jobPostId)
        {
            var applicants = await _jobRepo.GetApplicantsByJobIdAsync(partnerUserId, jobPostId);
            return Result<List<ApplicantListDto>>.Success(applicants.Select(a => new ApplicantListDto
            {
                ApplicationId = a.JobApplicationId, StudentName = a.User.Student?.Name ?? a.User.Email,
                StudentEmail = a.User.Email,
                AppliedDate = MyanmarTime.ToMyanmar(a.AppliedDate), CoverLetter = a.CoverLetter,
                ResumeUrl = a.ResumeUrl, Status = a.Status
            }).ToList());
        }

        public async Task<Result<bool>> UpdateApplicationStatusAsync(int applicationId, string newStatus)
        {
            if (newStatus is not ("Shortlisted" or "Rejected" or "Pending")) return Result<bool>.Failure("Invalid application status.");

            if (newStatus == "Rejected")
            {
                var application = await _jobRepo.GetApplicationByIdAsync(applicationId);
                if (application is not null)
                {
                    string studentName = application.User.Student?.Name ?? application.User.Email;
                    string body =
                        $"<p>Dear {studentName},</p>" +
                        $"<p>Thank you for applying to the position of <strong>{application.JobPost.Title}</strong> at <strong>{application.JobPost.CompanyName}</strong>.</p>" +
                        $"<p>After careful consideration, we regret to inform you that your application has <strong>not been shortlisted</strong> for this role.</p>" +
                        $"<p>We encourage you to keep applying to other opportunities on the PUPL Alumni &amp; Career Network.</p>" +
                        $"<p>&mdash; PUPL AJOCNS Team</p>";

                    try
                    {
                        await _emailService.SendEmailAsync(application.User.Email, $"Application Status — {application.JobPost.Title}", body);
                    }
                    catch
                    {
                        // email failure should not block the status update
                    }
                }
            }

            return await _jobRepo.UpdateApplicationStatusAsync(applicationId, newStatus)
                ? Result<bool>.Success(true) : Result<bool>.Failure("Application not found.");
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
            PostedDate = MyanmarTime.ToMyanmar(j.PostedDate),
            ClosingDate = MyanmarTime.ToMyanmar(j.ClosingDate),
            Status = j.Status,
            PostedByName = j.PostedByUser?.Email ?? "Unknown"
            ,PostedByRole = j.PostedByUser?.Role ?? string.Empty
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
                ClosingDate = MyanmarTime.ToMyanmar(jobPost.ClosingDate)
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

        public async Task<Result<List<JobPostDto>>> GetOpenJobsAsync()
        {
            var jobPosts = await _jobRepo.GetOpenJobPostsAsync();
            if (jobPosts == null)
            {
                return Result<List<JobPostDto>>.Success(new List<JobPostDto>());
            }

            var jobPostDtos = jobPosts
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

        public async Task<Result<PagedJobPostDto>> GetJobPostsPagedForUserAsync(int userId, int page, int pageSize, string? jobType = null, string? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _jobRepo.GetJobPostsPagedForUserAsync(userId, page, pageSize, jobType, status);

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

        public async Task<Result<List<JobPostDto>>> GetPendingJobPostsAsync()
        {
            var jobPosts = await _jobRepo.GetPendingJobPostsAsync();
            if (jobPosts == null || !jobPosts.Any())
            {
                return Result<List<JobPostDto>>.Success(new List<JobPostDto>());
            }

            var jobPostDtos = jobPosts
                .Select(j => BuildJobPostDto(j))
                .ToList();

            return Result<List<JobPostDto>>.Success(jobPostDtos);
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