using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class AcacdmicYear
{
    public int AcyId { get; set; }

    public string AcademicYear { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
