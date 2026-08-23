using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class Major
{
    public int MajorId { get; set; }

    public string MajorName { get; set; } = null!;

    public bool? IsFoundation { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
