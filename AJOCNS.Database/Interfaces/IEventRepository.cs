using AJOCNS.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Database.Interfaces
{
    public interface IEventRepository
    {
        Task<List<EventType>> GetEventTypesAsync();
        Task<bool> EventTypeExistsAsync(int eventTypeId);
        Task<bool> CreateEventAsync(Event newEvent);
        Task<List<Event>> GetAllEventsAsync();
        Task<(List<Event> Items, int TotalCount)> GetEventsPagedAsync(int page, int pageSize);
        Task<bool> UpdateEventStatusAsync(int eventId, string status);
        Task<int> CountPendingEventsAsync();
    }
}
