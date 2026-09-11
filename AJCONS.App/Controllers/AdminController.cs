using ajocns.database.interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Auth;
using AJOCNS.Shared.DTOs.Events;
using AJOCNS.Shared.DTOs.GraduationRecords;
using AJOCNS.Shared.DTOs.Jobs;
using AJOCNS.Shared.DTOs.Mentor;
using AJOCNS.Shared.DTOs.StudentRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IStudentRegistrationService _studentRegistrationService;
        private readonly IGraduationRecordService _graduationRecordService;
        private readonly IAuthService _authService;
        private readonly IEventService _eventService;
        private readonly IJobService _jobService;
        private readonly IMentorService _mentorService;
        private readonly IAuthRepository _authRepository;
        public AdminController(IStudentRegistrationService studentRegistrationService, IGraduationRecordService graduationRecordService, IAuthService authService, IEventService eventService, IJobService jobService, IMentorService mentorService, IAuthRepository authRepository)
        {
            _studentRegistrationService = studentRegistrationService;
            _graduationRecordService = graduationRecordService;
            _authService = authService;
            _eventService = eventService;
            _jobService = jobService;
            _mentorService = mentorService;
            _authRepository = authRepository;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _studentRegistrationService.GetDashboardStatsAsync();
            ViewBag.ActiveStudentCount = stats.IsSuccess ? stats.Data.ActiveStudents : 0;
            ViewBag.ActiveMentorCount = stats.IsSuccess ? stats.Data.ActiveMentors : 0;
            ViewBag.CareerEventCount = stats.IsSuccess ? stats.Data.CareerEventsHosted : 0;

            var pendingUsers = await _authService.GetPendingUsersAsync();
            ViewBag.PendingUsers = pendingUsers.IsSuccess ? pendingUsers.Data : new List<PendingUserApprovalDto>();
            ViewBag.PendingApprovalCount = pendingUsers.IsSuccess ? pendingUsers.Data.Count : 0;

            var pendingEvents = await _eventService.GetPendingEventsAsync();
            var pendingJobs = await _jobService.GetPendingJobPostsAsync();
            ViewBag.PendingPostApprovalCount =
                (pendingEvents.IsSuccess ? pendingEvents.Data.Count : 0)
                + (pendingJobs.IsSuccess ? pendingJobs.Data.Count : 0);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PendingEventDetails(int id)
        {
            var result = await _eventService.GetPendingEventsAsync();
            var item = result.IsSuccess ? result.Data.FirstOrDefault(e => e.Id == id) : null;
            return item is null ? NotFound() : PartialView("~/Views/Event/_EventDetailsModal.cshtml", item);
        }

        [HttpGet]
        public async Task<IActionResult> PendingJobDetails(int id)
        {
            var result = await _jobService.GetPendingJobPostsAsync();
            var item = result.IsSuccess ? result.Data.FirstOrDefault(j => j.Id == id) : null;
            return item is null ? NotFound() : PartialView("~/Views/Student/_JobDetailsModal.cshtml", item);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalPartners()
        {
            var result = await _authService.GetExternalPartnersAsync();
            return View(result.IsSuccess ? result.Data : new List<ExternalPartnerAdminDto>());
        }

        [HttpGet]
        public async Task<IActionResult> ExternalPartnerDetails(int userId, string? returnAction = null)
        {
            var result = await _authService.GetPendingExternalPartnerAsync(userId);
            if (!result.IsSuccess)
            {
                var allPartners = await _authService.GetExternalPartnersAsync();
                var partner = allPartners.IsSuccess
                    ? allPartners.Data.FirstOrDefault(p => p.UserId == userId)
                    : null;
                if (partner is null)
                {
                    TempData["SweetAlert_Type"] = "error";
                    TempData["SweetAlert_Title"] = "Partner not found";
                    TempData["SweetAlert_Message"] = "The requested external partner could not be found.";
                    return RedirectToAction(nameof(ExternalPartners));
                }
                ViewBag.IsPending = false;
                ViewBag.ReturnAction = returnAction;
                return View(partner);
            }

            ViewBag.IsPending = true;
            ViewBag.ReturnAction = returnAction;
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExternalPartnerStatus(int userId, string status)
        {
            if (status is not ("Active" or "Inactive"))
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Invalid status";
                TempData["SweetAlert_Message"] = "The requested status is not valid.";
                return RedirectToAction(nameof(ExternalPartners));
            }

            var result = await _authService.UpdateExternalPartnerStatusAsync(userId, status);
            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Status updated" : "Update failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "External partner status updated successfully."
                : result.ErrorMessage ?? "Could not update external partner status.";

            return RedirectToAction(nameof(ExternalPartners));
        }


        [HttpGet]
        public async Task<IActionResult> StudentManagement(int page = 1, int? majorId = null, int? acyId = null, bool excludeDropout = false)
        {
            const int pageSize = 10;

            var majors = await _studentRegistrationService.GetMajorsAsync();
            ViewBag.Majors = majors;
            ViewBag.SelectedMajorId = majorId;

            var academicYears = await _studentRegistrationService.GetAcademicYearsAsync();
            ViewBag.AcademicYears = academicYears;
            ViewBag.SelectedAcyId = acyId;

            var studentStats = await _studentRegistrationService.GetStudentStatusStatsAsync();
            ViewBag.StudentStatusStats = studentStats.IsSuccess ? studentStats.Data : null;

            var result = await _studentRegistrationService.GetStudentsPagedAsync(
                page, pageSize, majorId, acyId,
                excludeDropout ? "Dropout" : null);
            if (!result.IsSuccess)
            {
                return View(new PagedStudentDto());
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateMajors([FromBody] List<BulkMajorUpdateItemDto> updates)
        {
            var result = await _studentRegistrationService.BulkUpdateMajorsAsync(updates);
            if (result.IsSuccess)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = result.ErrorMessage ?? "Failed to update majors." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateGraduations([FromBody] BulkGraduationUpdateRequestDto request)
        {
            var result = await _studentRegistrationService.BulkUpdateGraduationsAsync(request);
            if (result.IsSuccess)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = result.ErrorMessage ?? "Failed to update graduation statuses." });
        }

        [HttpGet]
        public async Task<IActionResult> GraduationRecords(int page = 1, short? graduationYear = null, string? degreeCode = null)
        {
            const int pageSize = 10;

            var years = await _graduationRecordService.GetGraduationYearsAsync();
            ViewBag.GraduationYears = years;
            ViewBag.SelectedGraduationYear = graduationYear;

            await PopulateDegrees();
            ViewBag.SelectedDegreeCode = degreeCode;

            var result = await _graduationRecordService.GetGraduationRecordsPagedAsync(page, pageSize, degreeCode, graduationYear);
            if (!result.IsSuccess)
            {
                return View(new PagedGraduationRecordDto());
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGraduationRecord(int id)
        {
            var result = await _graduationRecordService.DeleteGraduationRecordAsync(id);

            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Deleted!" : "Delete Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Graduation record has been removed."
                : result.ErrorMessage ?? "Could not delete graduation record.";

            return RedirectToAction("GraduationRecords", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> EditGraduationRecord(int id)
        {
            var result = await _graduationRecordService.GetGraduationRecordByIdAsync(id);
            if (!result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Not Found";
                TempData["SweetAlert_Message"] = result.ErrorMessage;
                return RedirectToAction("GraduationRecords", "Admin");
            }

            await PopulateDegrees();
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGraduationRecord(EditGraduationRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDegrees();
                return View(dto);
            }

            var result = await _graduationRecordService.UpdateGraduationRecordAsync(dto);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Updated!";
                TempData["SweetAlert_Message"] = "Graduation record updated successfully.";
                return RedirectToAction("GraduationRecords", "Admin");
            }

            TempData["SweetAlert_Type"] = "error";
            TempData["SweetAlert_Title"] = "Update Failed";
            TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Could not update graduation record.";
            await PopulateDegrees();
            return View(dto);
        }

        private async Task PopulateDegrees()
        {
            var degrees = await _graduationRecordService.GetDegreesAsync();
            if (degrees.IsSuccess)
            {
                ViewBag.Degrees = degrees;
            }
            else
            {
                ModelState.AddModelError("", "No degrees found.");
                ViewBag.Degrees = new List<DegreeOptionDto>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> RegisterNewStudent()
        {
            await PopulateFoundationMajors();
            await PopulateAcademicYears();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StudentRegistrationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _studentRegistrationService.RegisterStudentAsync(dto);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Registered!";
                TempData["SweetAlert_Message"] = "Student registered and email sent successfully!";
                return RedirectToAction("StudentManagement", "Admin");
            }

            TempData["SweetAlert_Type"] = "error";
            TempData["SweetAlert_Title"] = "Registration Failed";
            TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Email might already exist.";
            await PopulateFoundationMajors();
            await PopulateAcademicYears();
            return View("RegisterNewStudent",dto);
        }

        private async Task PopulateMajorsDropdownAsync()
        {
            var majors = await _studentRegistrationService.GetMajorsAsync();
            if (majors.IsSuccess)
            {
                ViewBag.Majors = majors;
            }
            else
            {
                ModelState.AddModelError("", "No majors found.");
                ViewBag.Majors = new List<string>();
            }
        }

        private async Task PopulateFoundationMajors()
        {
            var foundationMajors = await _studentRegistrationService.GetFoundationMajorsAsync();
            if (foundationMajors.IsSuccess)
            {
                ViewBag.FoundationMajors = foundationMajors;
            }
            else
            {
                ModelState.AddModelError("", "No foundation majors found.");
                ViewBag.FoundationMajors = new List<string>();
            }
        }
        private async Task PopulateAcademicYears()
        {
            var acs = await _studentRegistrationService.GetAcademicYearsAsync();
            if (acs.IsSuccess)
            {
                ViewBag.AcademicYears = acs;
            }
            else
            {
                ModelState.AddModelError("", "No enrollment year found.");
                ViewBag.AcademicYears = new List<string>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var result = await _studentRegistrationService.GetStudentByIdAsync(id);
            if (!result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Not Found";
                TempData["SweetAlert_Message"] = result.ErrorMessage;
                return RedirectToAction("StudentManagement", "Admin");
            }

            await PopulateMajorsDropdownAsync();
            await PopulateAcademicYears();
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(EditStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateMajorsDropdownAsync();
                await PopulateAcademicYears();
                return View(dto);
            }

            var result = await _studentRegistrationService.UpdateStudentAsync(dto);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Updated!";
                TempData["SweetAlert_Message"] = "Student record updated successfully.";
                return RedirectToAction("StudentManagement", "Admin");
            }

            TempData["SweetAlert_Type"] = "error";
            TempData["SweetAlert_Title"] = "Update Failed";
            TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Could not update student.";
            await PopulateMajorsDropdownAsync();
            await PopulateAcademicYears();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var result = await _studentRegistrationService.DeleteStudentAsync(id);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Deleted!";
                TempData["SweetAlert_Message"] = "Student has been removed.";
            }
            else
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Delete Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Could not delete student.";
            }

            return RedirectToAction("StudentManagement", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> UserApprovals(string? role = null)
        {
            var result = await _authService.GetPendingUsersAsync();
            var pendingUsers = result.IsSuccess ? result.Data : new List<PendingUserApprovalDto>();

            if (!string.IsNullOrEmpty(role) && !string.Equals(role, "All", StringComparison.OrdinalIgnoreCase))
            {
                pendingUsers = pendingUsers
                    .Where(u => string.Equals(u.Role, role, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.PendingUsers = pendingUsers;
            ViewBag.SelectedRole = string.IsNullOrEmpty(role) ? "All" : role;

            var stats = await _studentRegistrationService.GetDashboardStatsAsync();
            ViewBag.ActiveStudentCount = stats.IsSuccess ? stats.Data.ActiveStudents : 0;
            ViewBag.ActiveMentorCount = stats.IsSuccess ? stats.Data.ActiveMentors : 0;
            ViewBag.PendingApprovalCount = stats.IsSuccess ? stats.Data.PendingApprovals : 0;
            ViewBag.CareerEventCount = stats.IsSuccess ? stats.Data.CareerEventsHosted : 0;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(int userId)
        {
            var result = await _authService.ApproveUserAsync(userId);

            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Approved!" : "Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "User has been approved and can now log in."
                : result.ErrorMessage ?? "Could not approve user.";

            return RedirectToAction("UserApprovals", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(int userId)
        {
            var result = await _authService.RejectUserAsync(userId);

            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Rejected" : "Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "User has been rejected and cannot log in."
                : result.ErrorMessage ?? "Could not reject user.";

            return RedirectToAction("UserApprovals", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyUser(int userId)
        {
            var result = await _authService.ApproveUserAsync(userId);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Applicant verified. Confirmation email sent and account approved." });
            }

            return Json(new { success = false, message = result.ErrorMessage ?? "Could not verify applicant." });
        }

        [HttpGet]
        public async Task<IActionResult> ContentApprovals()
        {
            var pendingEvents = await _eventService.GetPendingEventsAsync();
            ViewBag.PendingEvents = pendingEvents.IsSuccess ? pendingEvents.Data : new List<EventDto>();

            var pendingJobs = await _jobService.GetPendingJobPostsAsync();
            ViewBag.PendingJobs = pendingJobs.IsSuccess ? pendingJobs.Data : new List<JobPostDto>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEventPost(int id)
        {
            var result = await _eventService.ApproveEventAsync(id);

            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Approved!" : "Approval Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Event has been approved and is now upcoming."
                : result.ErrorMessage ?? "Could not approve event.";

            return RedirectToAction("ContentApprovals", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEventPost(int id)
        {
            var result = await _eventService.RejectEventAsync(id);

            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Rejected" : "Rejection Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Event has been rejected."
                : result.ErrorMessage ?? "Could not reject event.";

            return RedirectToAction("ContentApprovals", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveJobPost(int id)
        {
            var result = await _jobService.ApproveJobPostAsync(id);

            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Approved!" : "Approval Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Job post has been approved and is now open."
                : result.ErrorMessage ?? "Could not approve job post.";

            return RedirectToAction("ContentApprovals", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectJobPost(int id)
        {
            var result = await _jobService.RejectJobPostAsync(id);

            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Rejected" : "Rejection Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Job post has been rejected."
                : result.ErrorMessage ?? "Could not reject job post.";

            return RedirectToAction("ContentApprovals", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> Mentors()
        {
            var result = await _mentorService.GetAllMentorsAsync();
            var mentors = result.IsSuccess ? result.Data : new List<MentorProfileDto>();
            mentors = mentors
                .Where(m => !string.Equals(m.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                .ToList();
            return View(mentors);
        }

        [HttpGet]
        public async Task<IActionResult> MentorDetails(int userId)
        {
            var result = await _mentorService.GetMentorProfileAsync(userId);
            if (!result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Mentor not found";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "The requested mentor could not be found.";
                return RedirectToAction(nameof(Mentors));
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateMentor(int id)
        {
            var mentor = await _mentorService.GetMentorProfileAsync(id);
            if (!mentor.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Not Found";
                TempData["SweetAlert_Message"] = "Mentor account could not be found.";
                return RedirectToAction("Mentors", "Admin");
            }

            var updated = await _authRepository.UpdateUserStatusAsync(id, "Inactive");

            TempData["SweetAlert_Type"] = updated ? "success" : "error";
            TempData["SweetAlert_Title"] = updated ? "Deactivated" : "Update Failed";
            TempData["SweetAlert_Message"] = updated
                ? $"{mentor.Data.Name} can no longer sign in until reactivated."
                : "Could not deactivate the mentor account.";

            return RedirectToAction("Mentors", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateMentor(int id)
        {
            var mentor = await _mentorService.GetMentorProfileAsync(id);
            if (!mentor.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Not Found";
                TempData["SweetAlert_Message"] = "Mentor account could not be found.";
                return RedirectToAction("Mentors", "Admin");
            }

            var updated = await _authRepository.UpdateUserStatusAsync(id, "Active");

            TempData["SweetAlert_Type"] = updated ? "success" : "error";
            TempData["SweetAlert_Title"] = updated ? "Activated" : "Update Failed";
            TempData["SweetAlert_Message"] = updated
                ? $"{mentor.Data.Name} can now sign in again."
                : "Could not activate the mentor account.";

            return RedirectToAction("Mentors", "Admin");
        }
    }
}
