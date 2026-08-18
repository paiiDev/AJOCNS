using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class Mentor
{
    public int MentorId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public int GrecordId { get; set; }

    public virtual ICollection<EmploymentRecord> EmploymentRecords { get; set; } = new List<EmploymentRecord>();

    public virtual GraduationRecord Grecord { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
