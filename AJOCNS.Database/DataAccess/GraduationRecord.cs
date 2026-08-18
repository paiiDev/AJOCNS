using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class GraduationRecord
{
    public int GrecordId { get; set; }

    public string OfficialName { get; set; } = null!;

    public string? Uni { get; set; }

    public string Grn { get; set; } = null!;

    public short GraduationYear { get; set; }

    public string Degree { get; set; } = null!;

    public string AccStatus { get; set; } = null!;

    public virtual Mentor? Mentor { get; set; }

    public virtual Student? Student { get; set; }
}
