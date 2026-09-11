using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Events;
using AJOCNS.Shared.DTOs.Jobs;
using AJOCNS.Shared.DTOs.Mentor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "Mentor")]
    public class MentorController : Controller
    {
        private readonly IMentorService _mentorService;
        private readonly IEventService _eventService;
        private readonly IJobService _jobService;

        public MentorController(IMentorService mentorService, IEventService eventService, IJobService jobService)
        {
            _mentorService = mentorService;
            _eventService = eventService;
            _jobService = jobService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var profileResult = await _mentorService.GetMentorProfileAsync(userId);
            if (!profileResult.IsSuccess)
            {
                ViewBag.ShowEmploymentAlert = false;
                return View(new MentorProfileDto());
            }
            ViewBag.Profile = profileResult.Data;
            ViewBag.ShowEmploymentAlert = !profileResult.Data.HasEmploymentRecords;
            return View(profileResult.Data);
        }

        public async Task<IActionResult> Events(int page = 1, string? eventType = null, string? eventStatus = null)
        {
            var userId = GetCurrentUserId();
            var profileResult = await _mentorService.GetMentorProfileAsync(userId);
            ViewBag.Profile = profileResult.IsSuccess ? profileResult.Data : null;
            ViewBag.ShowEmploymentAlert = profileResult.IsSuccess && !profileResult.Data.HasEmploymentRecords;

            var result = await _eventService.GetEventsPagedForUserAsync(userId, page, 10, eventType, eventStatus);
            return View(result.IsSuccess ? result.Data : new PagedEventDto());
        }

        public async Task<IActionResult> Jobs(int page = 1)
        {
            var userId = GetCurrentUserId();
            var profileResult = await _mentorService.GetMentorProfileAsync(userId);
            ViewBag.Profile = profileResult.IsSuccess ? profileResult.Data : null;
            ViewBag.ShowEmploymentAlert = profileResult.IsSuccess && !profileResult.Data.HasEmploymentRecords;

            var result = await _jobService.GetJobPostsPagedForUserAsync(userId, page, 10);
            return View(result.IsSuccess ? result.Data : new PagedJobPostDto());
        }

        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            var profileResult = await _mentorService.GetMentorProfileAsync(userId);
            if (!profileResult.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Error";
                TempData["SweetAlert_Message"] = profileResult.ErrorMessage ?? "Failed to load profile";
                return RedirectToAction("Index");
            }

            ViewBag.ShowEmploymentAlert = !profileResult.Data.HasEmploymentRecords;

            return View(profileResult.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(MentorProfileEditDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Validation Error";
                TempData["SweetAlert_Message"] = "Please fill all required fields";
                return RedirectToAction("Profile");
            }

            var userId = GetCurrentUserId();
            var result = await _mentorService.UpdateMentorProfileAsync(userId, dto);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Updated";
                TempData["SweetAlert_Message"] = "Profile updated successfully";
            }
            else
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Failed to update profile";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmploymentRecord(CreateEmploymentRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Validation Error";
                TempData["SweetAlert_Message"] = "Please fill all required fields";
                return RedirectToAction("Profile");
            }

            var userId = GetCurrentUserId();
            var profileResult = await _mentorService.GetMentorProfileAsync(userId);
            if (!profileResult.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Error";
                TempData["SweetAlert_Message"] = "Mentor profile not found";
                return RedirectToAction("Profile");
            }

            var result = await _mentorService.CreateEmploymentRecordAsync(profileResult.Data.MentorId, dto);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Added";
                TempData["SweetAlert_Message"] = "Employment record added successfully";
            }
            else
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Failed to add employment record";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmploymentRecord(UpdateEmploymentRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Validation Error";
                TempData["SweetAlert_Message"] = "Please fill all required fields";
                return RedirectToAction("Profile");
            }

            var userId = GetCurrentUserId();
            var profileResult = await _mentorService.GetMentorProfileAsync(userId);
            if (!profileResult.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Error";
                TempData["SweetAlert_Message"] = "Mentor profile not found";
                return RedirectToAction("Profile");
            }

            var result = await _mentorService.UpdateEmploymentRecordAsync(profileResult.Data.MentorId, dto);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Updated";
                TempData["SweetAlert_Message"] = "Employment record updated successfully";
            }
            else
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Failed to update employment record";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmploymentRecord(int id)
        {
            var userId = GetCurrentUserId();
            var profileResult = await _mentorService.GetMentorProfileAsync(userId);
            if (!profileResult.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Error";
                TempData["SweetAlert_Message"] = "Mentor profile not found";
                return RedirectToAction("Profile");
            }

            var result = await _mentorService.DeleteEmploymentRecordAsync(profileResult.Data.MentorId, id);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Deleted";
                TempData["SweetAlert_Message"] = "Employment record deleted successfully";
            }
            else
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Failed to delete employment record";
            }

            return RedirectToAction("Profile");
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }
    }
}