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
        Task<bool> UpdateEventAsync(Event ev);
        Task<bool> DeleteEventAsync(int id);
        Task<List<Event>> GetAllEventsAsync();
        Task<Event?> GetEventById(int? id);
        Task<(List<Event> Items, int TotalCount)> GetEventsPagedAsync(int page, int pageSize, string? eventType = null, string? eventStatus = null);
        Task<bool> UpdateEventStatusAsync(int eventId, string status);
        Task<int> CountPendingEventsAsync();
        Task<List<Event>> GetEventStatusesAsync();
        Task<bool> IsStudentRegisteredAsync(int eventId, int studentId);
        Task<int> CountEventRegistrationsAsync(int eventId);
        Task<List<int>> GetStudentRegisteredEventIdsAsync(int studentId);
        Task<Dictionary<int, int>> GetEventRegistrationCountsAsync();
        Task<bool> AddEventRegistrationAsync(EventRegistration registration);
        Task<List<EventRegistration>> GetEventRegistrationsWithStudentsAsync(int eventId);
    }
}
