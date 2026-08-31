using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.StudentDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AJOCNS.App.Controllers
{
    //[Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
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

            return View(result.Data);
        }

        public IActionResult CareerBuilder()
        {
            return View();
        }
    }
}
