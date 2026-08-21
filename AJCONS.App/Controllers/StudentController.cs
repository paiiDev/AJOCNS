using Microsoft.AspNetCore.Mvc;

namespace AJOCNS.App.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


    }
}
