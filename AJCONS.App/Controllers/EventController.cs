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
                    posterPath = await UploadPosterAsync(dto.PosterImage);
                    if (posterPath == null)
                    {
                        ModelState.AddModelError(nameof(dto.PosterImage), "Could not upload poster image.");
                        await PopulateEventTypes();
                        return View(dto);
                    }
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

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var result = await _eventService.GetEventForEditAsync(id);
            if (!result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Not Found";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Event could not be found.";
                return RedirectToAction("Index", "Event");
            }

            await PopulateEventTypes();
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(UpdateEventDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEventTypes();
                return View(dto);
            }

            string? newPosterPath = null;
            if (dto.PosterImage != null)
            {
                newPosterPath = await UploadPosterAsync(dto.PosterImage);
                if (newPosterPath == null)
                {
                    ModelState.AddModelError(nameof(dto.PosterImage), "Could not upload poster image.");
                    await PopulateEventTypes();
                    return View(dto);
                }
            }

            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            bool isAdmin = User.IsInRole("Admin");
            var eventDateUtc = TimeZoneInfo.ConvertTimeToUtc(dto.EventDate, MyanmarTimeZone);

            var result = await _eventService.UpdateEventAsync(dto, currentUserId, isAdmin, eventDateUtc, newPosterPath);
            if (result.IsSuccess)
            {
                if (!string.IsNullOrEmpty(newPosterPath))
                {
                    DeletePosterFile(dto.CurrentPosterPath);
                }

                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Updated!";
                TempData["SweetAlert_Message"] = "Event has been updated successfully.";
                return RedirectToAction("Index", "Event");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update event.");
            await PopulateEventTypes();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventDetails = await _eventService.GetEventDetailsModal(id);

            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            bool isAdmin = User.IsInRole("Admin");

            var result = await _eventService.DeleteEventAsync(id, currentUserId, isAdmin);
            if (result.IsSuccess)
            {
                if (eventDetails.IsSuccess && !string.IsNullOrEmpty(eventDetails.Data?.PosterImagePath))
                {
                    DeletePosterFile(eventDetails.Data.PosterImagePath);
                }

                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Deleted!";
                TempData["SweetAlert_Message"] = "Event has been deleted successfully.";
            }
            else
            {
                TempData["SweetAlert_Type"] = "error";
                TempData["SweetAlert_Title"] = "Delete Failed";
                TempData["SweetAlert_Message"] = result.ErrorMessage ?? "Could not delete event.";
            }

            return RedirectToAction("Index", "Event");
        }

        private async Task<string?> UploadPosterAsync(IFormFile posterImage)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "events");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + posterImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await posterImage.CopyToAsync(fileStream);
                }
            }
            catch
            {
                return null;
            }

            return "/images/events/" + uniqueFileName;
        }

        private void DeletePosterFile(string? posterPath)
        {
            if (string.IsNullOrEmpty(posterPath))
            {
                return;
            }

            string relativePath = posterPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch
                {
                    // ignore file-level cleanup failures
                }
            }
        }

        private async Task PopulateEventTypes()
        {
            var types = await _eventService.GetEventTypesAsync();
            ViewBag.EventTypes = types.IsSuccess ? types.Data : new List<EventTypeDto>();
        }
    }
}
