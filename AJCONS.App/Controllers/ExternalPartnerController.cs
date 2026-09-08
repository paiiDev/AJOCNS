using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "ExternalPartner")]
    public class ExternalPartnerController : Controller
    {
        private readonly IEventService _eventService;

        public ExternalPartnerController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Events(int page = 1, string? eventType = null, string? eventStatus = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _eventService.GetEventsPagedForUserAsync(userId, page, 10, eventType, eventStatus);
            return View(result.IsSuccess ? result.Data : new PagedEventDto());
        }

        public IActionResult Jobs()
        {
            return View();
        }
    }
}