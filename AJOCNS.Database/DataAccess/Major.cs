using System;
using System.Collections.Generic;

namespace AJOCNS.Database.DataAccess;

public partial class Major
{
    public int MajorId { get; set; }

    public string MajorName { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
