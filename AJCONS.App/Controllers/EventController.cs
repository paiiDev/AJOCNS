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
        private static readonly TimeZoneInfo MyanmarTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
        private readonly IEventService _eventService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventController(IEventService eventService, IWebHostEnvironment webHostEnvironment)
        {
            _eventService = eventService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int page = 1, string? eventType = null, string? eventStatus = null)
        {
            const int pageSize = 10;


            var eventTypesResult = await _eventService.GetEventTypesAsync();
            ViewBag.EventTypes = eventTypesResult;
            ViewBag.SelectedEventType = eventType;

            var eventStatusesResult = await _eventService.GetEventStatusesAsync(); 
            ViewBag.EventStatuses = eventStatusesResult;
            ViewBag.SelectedEventStatus = eventStatus;

            var result = await _eventService.GetEventsPagedAsync(page, pageSize, eventType, eventStatus);
            ViewBag.IsAdmin = User.IsInRole("Admin");
           

            if (!result.IsSuccess)
            {
                return View(new PagedEventDto());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> GetEventDetailsModal(int id)
        {
            var eventDetails = await _eventService.GetEventDetailsModal(id);
            if (!eventDetails.IsSuccess)
            {
                return NotFound();
            }
            return PartialView("_EventDetailsModal", eventDetails.Data);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            await PopulateEventTypes();
            var nowMyanmar = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MyanmarTimeZone);
            var defaultEventDate = new DateTime(nowMyanmar.Year, nowMyanmar.Month, nowMyanmar.Day, nowMyanmar.Hour, nowMyanmar.Minute, 0);
            return View(new CreateEventDto { EventDate = defaultEventDate });
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


                string? posterPath = null;
                if (dto.PosterImage != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "events");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.PosterImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.PosterImage.CopyToAsync(fileStream);
                    }

                    posterPath = "/images/events" + uniqueFileName;
                }

                bool isAdmin = User.IsInRole("Admin");
                int createdByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                var eventDateMyanmar = dto.EventDate;
                var eventDateUtc = TimeZoneInfo.ConvertTimeToUtc(eventDateMyanmar, MyanmarTimeZone);

                var result = await _eventService.CreateEventAsync(dto, createdByUserId, autoApprove: isAdmin, eventDateUtc: eventDateUtc, posterPath);

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
