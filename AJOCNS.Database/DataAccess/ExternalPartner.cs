using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class ExternalPartner
{
    public int ExternalPartnerId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public int CompanyId { get; set; }

    public string? Phone { get; set; }

    public string? Expertise { get; set; }

    public int PositionId { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Position Position { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
