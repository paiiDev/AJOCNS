using AJOCNS.Database.Context;
using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Database.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventType>> GetEventTypesAsync()
        {
            return await _context.EventTypes.AsNoTracking().ToListAsync();
        }

        public async Task<bool> EventTypeExistsAsync(int eventTypeId)
        {
            return await _context.EventTypes.AnyAsync(et => et.EventTypeId == eventTypeId);
        }

        public async Task<bool> CreateEventAsync(Event newEvent)
        {
            try
            {
                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEventAsync(Event ev)
        {
            try
            {
                _context.Events.Update(ev);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            try
            {
                var ev = await _context.Events.FindAsync(id);
                if (ev is null) return false;

                ev.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Event?> GetEventById(int? id)
        {
            return await _context.Events.Include(e => e.EventType)
                                        .Include(e => e.CreatedByUser).ThenInclude(u => u.Admin)
                                        .Include(e => e.CreatedByUser).ThenInclude(u => u.Mentor)
                                        .Include(e => e.CreatedByUser).ThenInclude(u => u.ExternalPartner).FirstOrDefaultAsync(x => x.EventId == id);
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .AsNoTracking()
                .Include(e => e.EventType)
                .Include(e => e.CreatedByUser)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<List<Event>> GetEventStatusesAsync()
        {
               return await _context.Events
                .AsNoTracking()
                .Select(e => new Event
                {
                    Status = e.Status
                }).Distinct()
                .ToListAsync();
        }

        public async Task<(List<Event> Items, int TotalCount)> GetEventsPagedAsync(int page, int pageSize, string? eventType = null, string? eventStatus = null)
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.EventType)
                .Include(e => e.CreatedByUser)
                .AsQueryable();

            if(!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(e => e.EventType.EventTypeName == eventType);
            }

            if(!string.IsNullOrEmpty(eventStatus))
            {
                query = query.Where(e => e.Status == eventStatus);
            }

            int totalCount = await query.CountAsync();

            var items = await query
               .OrderByDescending(e => e.Status == "Upcoming")
                .ThenBy(e => e.EventDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> UpdateEventStatusAsync(int eventId, string status)
        {
            try
            {
                var ev = await _context.Events.FindAsync(eventId);
                if (ev is null) return false;

                ev.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> CountPendingEventsAsync()
        {
            return await _context.Events
                .AsNoTracking()
                .CountAsync(e => e.Status.ToLower().Contains("pend"));
        }

        public async Task<bool> IsStudentRegisteredAsync(int eventId, int studentId)
        {
            return await _context.EventRegistrations
                .AsNoTracking()
                .AnyAsync(r => r.EventId == eventId && r.StudentId == studentId);
        }

        public async Task<int> CountEventRegistrationsAsync(int eventId)
        {
            return await _context.EventRegistrations
                .AsNoTracking()
                .CountAsync(r => r.EventId == eventId);
        }

        public async Task<List<int>> GetStudentRegisteredEventIdsAsync(int studentId)
        {
            return await _context.EventRegistrations
                .AsNoTracking()
                .Where(r => r.StudentId == studentId)
                .Select(r => r.EventId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetEventRegistrationCountsAsync()
        {
            return await _context.EventRegistrations
                .AsNoTracking()
                .GroupBy(r => r.EventId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<bool> AddEventRegistrationAsync(EventRegistration registration)
        {
            try
            {
                _context.EventRegistrations.Add(registration);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
