using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class Event
{
    public int EventId { get; set; }

    public int CreatedByUserId { get; set; }

    public string EventTitle { get; set; } = null!;

    public string? Description { get; set; }

    public int EventTypeId { get; set; }

    public DateTime EventDate { get; set; }

    public int? MaxCapacity { get; set; }

    public string? EventMode { get; set; }

    public string? Location { get; set; }

    public string Status { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public string? PosterImagePath { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public virtual EventType EventType { get; set; } = null!;
}
