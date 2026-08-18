using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class EventType
{
    public int EventTypeId { get; set; }

    public string EventType1 { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
