using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsFirstLogin { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ExternalPartner? ExternalPartner { get; set; }

    public virtual Mentor? Mentor { get; set; }

    public virtual Student? Student { get; set; }
}
