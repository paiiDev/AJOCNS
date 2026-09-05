using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "ExternalPartner")]
    public class ExternalPartnerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Jobs()
        {
            return View();
        }
    }
}