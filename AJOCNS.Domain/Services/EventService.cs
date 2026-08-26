using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepo;

        public EventService(IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
        }

        public async Task<Result<List<EventTypeDto>>> GetEventTypesAsync()
        {
            var types = await _eventRepo.GetEventTypesAsync();
            if (types == null || !types.Any())
            {
                return Result<List<EventTypeDto>>.Failure("No event types found");
            }

            var typeDtos = types.Select(et => new EventTypeDto
            {
                Id = et.EventTypeId,
                Name = et.EventTypeName
            }).ToList();

            return Result<List<EventTypeDto>>.Success(typeDtos);
        }

        public async Task<Result<bool>> CreateEventAsync(CreateEventDto dto, int createdByUserId, bool autoApprove)
        {
            if (dto is null)
                return Result<bool>.Failure("Invalid event data.");

            if (dto.EventDate < DateTime.Now.AddHours(-1))
                return Result<bool>.Failure("Event date cannot be in the past.");

            bool typeExists = await _eventRepo.EventTypeExistsAsync(dto.EventTypeId);
            if (!typeExists)
                return Result<bool>.Failure("Selected event type does not exist.");

            var newEvent = new Event
            {
                CreatedByUserId = createdByUserId,
                EventTitle = dto.EventTitle.Trim(),
                Description = dto.Description,
                EventTypeId = dto.EventTypeId,
                EventDate = dto.EventDate,
                MaxCapacity = dto.MaxCapacity,
                EventMode = dto.EventMode,
                Location = dto.Location,
                Status = autoApprove ? "Upcoming" : "Pending",
                IsDeleted = false
            };

            bool saved = await _eventRepo.CreateEventAsync(newEvent);
            if (!saved)
                return Result<bool>.Failure("Failed to create event.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<List<EventDto>>> GetAllEventsAsync()
        {
            var events = await _eventRepo.GetAllEventsAsync();
            var eventDtos = (events ?? new List<Event>()).Select(e => new EventDto
            {
                Id = e.EventId,
                EventTitle = e.EventTitle,
                Description = e.Description,
                EventTypeName = e.EventType?.EventTypeName ?? "-",
                EventDate = e.EventDate,
                MaxCapacity = e.MaxCapacity,
                EventMode = e.EventMode,
                Location = e.Location,
                Status = e.Status,
                CreatedByName = e.CreatedByUser?.Email ?? "Unknown"
            }).ToList();

            return Result<List<EventDto>>.Success(eventDtos);
        }

        public async Task<Result<List<EventStatusDto>>> GetEventStatusesAsync()
        {
            var statuses = await _eventRepo.GetEventStatusesAsync();
            if (statuses == null || !statuses.Any())
            {
                return Result<List<EventStatusDto>>.Failure("No event statuses found");
            }
            var statusDtos = statuses.Select(s => new EventStatusDto
            {
                Status = s.Status
            }).ToList();
            return Result<List<EventStatusDto>>.Success(statusDtos);
        }

        public async Task<Result<PagedEventDto>> GetEventsPagedAsync(int page, int pageSize, string? eventType = null, string? eventStatus = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _eventRepo.GetEventsPagedAsync(page, pageSize, eventType, eventStatus);

            var paged = new PagedEventDto
            {
                Events = (items ?? new List<Event>()).Select(e => new EventDto
                {
                    Id = e.EventId,
                    EventTitle = e.EventTitle,
                    Description = e.Description,
                    EventTypeName = e.EventType?.EventTypeName ?? "-",
                    EventDate = e.EventDate,
                    MaxCapacity = e.MaxCapacity,
                    EventMode = e.EventMode,
                    Location = e.Location,
                    Status = e.Status,
                    CreatedByName = e.CreatedByUser?.Email ?? "Unknown"
                }).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Result<PagedEventDto>.Success(paged);
        }

        public async Task<Result<bool>> ApproveEventAsync(int eventId)
        {
            bool updated = await _eventRepo.UpdateEventStatusAsync(eventId, "Upcoming");
            if (!updated)
                return Result<bool>.Failure("Failed to approve event.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectEventAsync(int eventId)
        {
            bool updated = await _eventRepo.UpdateEventStatusAsync(eventId, "Rejected");
            if (!updated)
                return Result<bool>.Failure("Failed to reject event.");

            return Result<bool>.Success(true);
        }
    }
}
