using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Events;
using AJOCNS.Shared.DTOs.Student;
using AJOCNS.Shared.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IEventService _eventService;
        private readonly IStudentRepository _studentRepository;

        public StudentController(IStudentService studentService, IEventService eventService, IStudentRepository studentRepository)
        {
            _studentService = studentService;
            _eventService = eventService;
            _studentRepository = studentRepository;
        }

        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _studentService.GetStudentDashboardAsync(userId);
            if (!result.IsSuccess)
            {
                return View(new StudentDashboardDto
                {
                    Name = User.Identity?.Name ?? "Student",
                    Srn = "-",
                    Major = "-",
                    GraduationStatus = "Undergraduate"
                });
            }

            var registeredEvents = await _eventService.GetStudentRegisteredEventsAsync(result.Data.StudentId);
            ViewBag.RegisteredEvents = registeredEvents.IsSuccess ? registeredEvents.Data : new List<EventDto>();

            return View(result.Data);
        }

        [HttpGet]
        public IActionResult FirstLoginSetup()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetUp(StudentFirstLoginSetupDto dto)
        {
           if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _studentService.SetupStudentFirstLoginAsync(userId, dto);
            if (!result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Failed to insert data";
                return RedirectToAction("Index", "Student");
            }
            return RedirectToAction("Index", "Student");

        }

        public IActionResult CareerBuilder()
        {
            return View();
        }

        public async Task<IActionResult> Event()
        {
            var student = await _studentRepository.GetStudentByUserIdAsync(GetCurrentUserId());
            if (student is null)
            {
                return View(new List<EventDto>());
            }

            var result = await _eventService.GetStudentEventsAsync(student.StudentId);
            return View(result.IsSuccess ? result.Data : new List<EventDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEvent(int id)
        {
            var student = await _studentRepository.GetStudentByUserIdAsync(GetCurrentUserId());
            if (student is null)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Failed";
                TempData["SweetAlert_Message"] = "Student profile not found.";
                return RedirectToAction("Event", "Student");
            }

            var result = await _eventService.RegisterStudentForEventAsync(id, student.StudentId);
            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Registered!" : "Registration Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "You have successfully registered for this event."
                : result.ErrorMessage ?? "Could not register for this event.";

            return RedirectToAction("Event", "Student");
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        public async Task<IActionResult> GetEventDetailsModal(int id)
        {
            var eventDetails = await _eventService.GetEventDetailsModal(id);
            if (!eventDetails.IsSuccess)
            {
                return NotFound();
            }

            return PartialView("~/Views/Event/_EventDetailsModal.cshtml", eventDetails.Data);
        }
    }
}
