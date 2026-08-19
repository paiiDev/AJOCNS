using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public virtual ICollection<EmploymentRecord> EmploymentRecords { get; set; } = new List<EmploymentRecord>();

    public virtual ICollection<ExternalPartner> ExternalPartners { get; set; } = new List<ExternalPartner>();
}
