using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class Mentor
{
    public int MentorId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Expertise { get; set; }

    public short AlumniGy { get; set; }

    public string AlumniGrn { get; set; } = null!;

    public virtual ICollection<EmploymentRecord> EmploymentRecords { get; set; } = new List<EmploymentRecord>();

    public virtual User User { get; set; } = null!;
}
