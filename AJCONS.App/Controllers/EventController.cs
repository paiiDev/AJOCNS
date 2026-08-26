using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AJOCNS.App.Controllers
{
    [Authorize(Roles = "Admin,Mentor")]
    public class EventController : Controller
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var result = await _eventService.GetEventsPagedAsync(page, pageSize);
            ViewBag.IsAdmin = User.IsInRole("Admin");

            if (!result.IsSuccess)
            {
                return View(new PagedEventDto());
            }
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            await PopulateEventTypes();
            return View(new CreateEventDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEventTypes();
                return View(dto);
            }

            bool isAdmin = User.IsInRole("Admin");
            int createdByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _eventService.CreateEventAsync(dto, createdByUserId, autoApprove: isAdmin);

            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = isAdmin ? "Created!" : "Submitted!";
                TempData["SweetAlert_Message"] = isAdmin
                    ? "Event has been created successfully."
                    : "Event submitted. It will be visible once approved by an admin.";
                return RedirectToAction("Index", "Event");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create event.");
            await PopulateEventTypes();
            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var result = await _eventService.ApproveEventAsync(id);
            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Approved!" : "Approval Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Event has been approved and is now upcoming."
                : result.ErrorMessage ?? "Could not approve event.";
            return RedirectToAction("Index", "Event");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEvent(int id)
        {
            var result = await _eventService.RejectEventAsync(id);
            TempData["SweetAlert_Type"] = result.IsSuccess ? "success" : "error";
            TempData["SweetAlert_Title"] = result.IsSuccess ? "Rejected" : "Rejection Failed";
            TempData["SweetAlert_Message"] = result.IsSuccess
                ? "Event has been rejected."
                : result.ErrorMessage ?? "Could not reject event.";
            return RedirectToAction("Index", "Event");
        }

        private async Task PopulateEventTypes()
        {
            var types = await _eventService.GetEventTypesAsync();
            ViewBag.EventTypes = types.IsSuccess ? types.Data : new List<EventTypeDto>();
        }
    }
}
