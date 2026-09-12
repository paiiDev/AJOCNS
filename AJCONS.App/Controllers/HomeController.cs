using System.Diagnostics;
using AJCONS.App.Models;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Events;
using Microsoft.AspNetCore.Mvc;

namespace AJCONS.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventService _eventService;

        public HomeController(ILogger<HomeController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Index", "Student");
                }
                if (User.IsInRole("ExternalPartner"))
                {
                    return RedirectToAction("Index", "ExternalPartner");
                }
                if (User.IsInRole("Mentor"))
                {
                    return RedirectToAction("Index", "Mentor");
                }
            }

var eventsResult = await _eventService.GetEventsPagedAsync(1, 3, eventStatus: "Upcoming");
            ViewBag.UpcomingEvents = eventsResult.IsSuccess ? eventsResult.Data.Events : new List<EventDto>();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
