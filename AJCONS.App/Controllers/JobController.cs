using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "Admin,Mentor,ExternalPartner")]
    public class JobController : Controller
    {
        private static readonly TimeZoneInfo MyanmarTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
        private static readonly List<string> JobTypes = new List<string>
        {
            "Full-time",
            "Part-time",
            "Contract",
            "Internship",
            "Temporary",
            "Remote"
        };

        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        public async Task<IActionResult> Index(int page = 1, string? jobType = null, string? status = null)
        {
            const int pageSize = 10;

            ViewBag.JobTypes = JobTypes;
            ViewBag.SelectedJobType = jobType;

            var jobStatusesResult = await _jobService.GetJobStatusesAsync();
            ViewBag.JobStatuses = jobStatusesResult;
            ViewBag.SelectedStatus = status;

            ViewBag.IsAdmin = User.IsInRole("Admin");

            var result = await _jobService.GetJobPostsPagedAsync(page, pageSize, jobType, status);

            if (!result.IsSuccess)
            {
                return View(new PagedJobPostDto());
            }

            return View(result.Data);
        }

        [HttpGet]
        public IActionResult CreateJobPost()
        {
            ViewBag.JobTypes = JobTypes;
            var nowMyanmar = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MyanmarTimeZone);
            var defaultClosingDate = nowMyanmar.AddDays(30);
            return View(new CreateJobPostDto { ClosingDate = defaultClosingDate });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJobPost(CreateJobPostDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.JobTypes = JobTypes;
                return View(dto);
            }

            bool isAdmin = User.IsInRole("Admin");
            int postedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var closingDateMyanmar = dto.ClosingDate;
            var closingDateUtc = TimeZoneInfo.ConvertTimeToUtc(closingDateMyanmar, MyanmarTimeZone);

            var result = await _jobService.CreateJobPostAsync(dto, postedByUserId, autoApprove: isAdmin, closingDateUtc);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = isAdmin ? "Created!" : "Submitted!";
                TempData["SweetAlert_Message"] = isAdmin
                    ? "Job post has been created successfully."
                    : "Job post submitted. It will be visible once approved by an admin.";
                return RedirectToAction("Index", "Job");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create job post.");
            ViewBag.JobTypes = JobTypes;
            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveJobPost(int id)
        {
            var result = await _jobService.ApproveJobPostAsync(id);
            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Approved!" : "Approval Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Job post has been approved and is now open."
                : result.ErrorMessage ?? "Could not approve job post.";
            return RedirectToAction("Index", "Job");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectJobPost(int id)
        {
            var result = await _jobService.RejectJobPostAsync(id);
            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Rejected" : "Rejection Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Job post has been rejected."
                : result.ErrorMessage ?? "Could not reject job post.";
            return RedirectToAction("Index", "Job");
        }

        [HttpGet]
        public async Task<IActionResult> EditJobPost(int id)
        {
            var result = await _jobService.GetJobPostForEditAsync(id);
            if (!result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Not Found";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Job post could not be found.";
                return RedirectToAction("Index", "Job");
            }

            ViewBag.JobTypes = JobTypes;
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJobPost(UpdateJobPostDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.JobTypes = JobTypes;
                return View(dto);
            }

            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            bool isAdmin = User.IsInRole("Admin");
            var closingDateUtc = TimeZoneInfo.ConvertTimeToUtc(dto.ClosingDate, MyanmarTimeZone);

            var result = await _jobService.UpdateJobPostAsync(dto, currentUserId, isAdmin, closingDateUtc);
            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Updated!";
                TempData["SweetAlert_Message"] = "Job post has been updated successfully.";
                return RedirectToAction("Index", "Job");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update job post.");
            ViewBag.JobTypes = JobTypes;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobPost(int id)
        {
            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            bool isAdmin = User.IsInRole("Admin");

            var result = await _jobService.DeleteJobPostAsync(id, currentUserId, isAdmin);
            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Deleted!";
                TempData["SweetAlert_Message"] = "Job post has been deleted successfully.";
            }
            else
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Delete Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Could not delete job post.";
            }

            return RedirectToAction("Index", "Job");
        }
    }
}