using AJOCNS.Domain.Interfaces;
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
        public AdminController(IStudentRegistrationService studentRegistrationService)
        {
            _studentRegistrationService = studentRegistrationService;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> StudentManagement()
        {
            var students = await _studentRegistrationService.GetAllStudentsAsync();
            if (!students.IsSuccess)
            {
                ModelState.AddModelError("", "No students found.");
                return View();
            }
            return View(students.Data);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterNewStudent()
        {
            var majors =  await _studentRegistrationService.GetMajorsAsync();
            if (!majors.IsSuccess)
            {
                ModelState.AddModelError("", "No majors found.");
                return View();
            }
            ViewBag.Majors = majors;
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
                TempData["SuccessMessage"] = "Student registered and email sent successfully!";
                return RedirectToAction("Index", "Admin");
            }

            ModelState.AddModelError("", "Registration failed. Email might already exist.");
            return View(dto);
        }


    }


}
