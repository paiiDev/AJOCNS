using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class AcademicYear
{
    public int AcyId { get; set; }

    public string AcademicYear1 { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
