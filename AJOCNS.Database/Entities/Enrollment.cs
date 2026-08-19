using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class Enrollment
{
    public int ErId { get; set; }

    public int AcyId { get; set; }

    public int StudentId { get; set; }

    public string Status { get; set; } = null!;

    public virtual AcademicYear Acy { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
