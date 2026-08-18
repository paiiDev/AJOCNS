using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class Admin
{
    public int AdminId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
