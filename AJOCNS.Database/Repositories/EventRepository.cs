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

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .AsNoTracking()
                .Include(e => e.EventType)
                .Include(e => e.CreatedByUser)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<(List<Event> Items, int TotalCount)> GetEventsPagedAsync(int page, int pageSize)
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.EventType)
                .Include(e => e.CreatedByUser)
                .AsQueryable();

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.EventDate)
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
    }
}
