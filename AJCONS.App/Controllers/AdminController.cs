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
           await PopulateMajorsDropdownAsync();
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
            await PopulateMajorsDropdownAsync();
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


        }


}
