using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class GraduationRecord
{
    public int GrecordId { get; set; }

    public string OfficialName { get; set; } = null!;

    public string Grn { get; set; } = null!;

    public short GraduationYear { get; set; }

    public int DegreeId { get; set; }

    public string AccStatus { get; set; } = null!;

    public int? StudentId { get; set; }

    public virtual Degree Degree { get; set; } = null!;

    public virtual Student? Student { get; set; }
}
