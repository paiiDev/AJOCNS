using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Interfaces
{
    public interface IEventService
    {
        Task<Result<List<EventTypeDto>>> GetEventTypesAsync();
        Task<Result<bool>> CreateEventAsync(CreateEventDto dto, int createdByUserId, bool autoApprove);
        Task<Result<List<EventDto>>> GetAllEventsAsync();
        Task<Result<PagedEventDto>> GetEventsPagedAsync(int page, int pageSize);
        Task<Result<bool>> ApproveEventAsync(int eventId);
        Task<Result<bool>> RejectEventAsync(int eventId);
    }
}
