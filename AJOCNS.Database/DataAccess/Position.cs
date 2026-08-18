using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class Position
{
    public int PositionId { get; set; }

    public string Position1 { get; set; } = null!;

    public virtual ICollection<EmploymentRecord> EmploymentRecords { get; set; } = new List<EmploymentRecord>();

    public virtual ICollection<ExternalPartner> ExternalPartners { get; set; } = new List<ExternalPartner>();
}
