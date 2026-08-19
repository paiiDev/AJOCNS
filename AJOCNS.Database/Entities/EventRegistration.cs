using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class EventRegistration
{
    public int EventRegiId { get; set; }

    public int EventId { get; set; }

    public int StudentId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
