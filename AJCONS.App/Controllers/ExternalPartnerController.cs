using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Events;
using AJOCNS.Shared.DTOs.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "ExternalPartner")]
    public class ExternalPartnerController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IJobService _jobService;
        private readonly IEmailService _emailService;

        public ExternalPartnerController(IEventService eventService, IJobService jobService, IEmailService emailService)
        {
            _eventService = eventService;
            _jobService = jobService;
            _emailService = emailService;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> Events(int page = 1, string? eventType = null, string? eventStatus = null)
        {
            var result = await _eventService.GetEventsPagedForUserAsync(GetCurrentUserId(), page, 10, eventType, eventStatus);
            return View(result.IsSuccess ? result.Data : new PagedEventDto());
        }

        public IActionResult Jobs() => RedirectToAction(nameof(MyJobPosts));

        public async Task<IActionResult> MyJobPosts()
        {
            var result = await _jobService.GetJobPostsPagedForUserAsync(GetCurrentUserId(), 1, 100);
            return View(result.IsSuccess ? result.Data.Jobs : new List<JobPostDto>());
        }

        public async Task<IActionResult> ViewApplicants(int jobPostId)
        {
            var result = await _jobService.GetApplicantsByJobIdAsync(GetCurrentUserId(), jobPostId);
            return result.IsSuccess ? View(result.Data) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicantStatus(int applicationId, string status, int jobPostId)
        {
            var applicants = await _jobService.GetApplicantsByJobIdAsync(GetCurrentUserId(), jobPostId);
            var applicant = applicants.Data?.FirstOrDefault(a => a.ApplicationId == applicationId);
            if (applicant is null) return Forbid();

            var result = await _jobService.UpdateApplicationStatusAsync(applicationId, status);
            if (result.IsSuccess && status == "Shortlisted" && applicant.Status != "Shortlisted")
            {
                await _emailService.SendEmailAsync(applicant.StudentEmail, "Your job application was shortlisted",
                    $"<p>Dear {applicant.StudentName},</p><p>Your application has been shortlisted. The company will contact you with next steps.</p>");
            }

            return RedirectToAction(nameof(ViewApplicants), new { jobPostId });
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}