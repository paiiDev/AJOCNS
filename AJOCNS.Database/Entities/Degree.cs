using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class Degree
{
    public int DegreeId { get; set; }

    public string DegreeName { get; set; } = null!;

    public string DegreeCode { get; set; } = null!;

    public virtual ICollection<GraduationRecord> GraduationRecords { get; set; } = new List<GraduationRecord>();
}
