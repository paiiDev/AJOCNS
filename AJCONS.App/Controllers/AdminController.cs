using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.GraduationRecords;
using AJOCNS.Shared.DTOs.StudentRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IStudentRegistrationService _studentRegistrationService;
        private readonly IGraduationRecordService _graduationRecordService;
        public AdminController(IStudentRegistrationService studentRegistrationService, IGraduationRecordService graduationRecordService)
        {
            _studentRegistrationService = studentRegistrationService;
            _graduationRecordService = graduationRecordService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _studentRegistrationService.GetDashboardStatsAsync();
            ViewBag.ActiveStudentCount = stats.IsSuccess ? stats.Data.ActiveStudents : 0;
            ViewBag.ActiveMentorCount = stats.IsSuccess ? stats.Data.ActiveMentors : 0;
            ViewBag.PendingApprovalCount = stats.IsSuccess ? stats.Data.PendingApprovals : 0;
            ViewBag.CareerEventCount = stats.IsSuccess ? stats.Data.CareerEventsHosted : 0;
            return View();
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
    }
}
