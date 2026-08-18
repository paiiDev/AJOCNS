using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class EmploymentRecord
{
    public int EmploymentRId { get; set; }

    public int MentorId { get; set; }

    public int CompanyId { get; set; }

    public int PositionId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Mentor Mentor { get; set; } = null!;

    public virtual Position Position { get; set; } = null!;
}
