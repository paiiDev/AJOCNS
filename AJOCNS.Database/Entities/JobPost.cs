using System;
using System.Collections.Generic;

namespace AJOCNS.Database.Entities;

public partial class JobPost
{
    public int JobPostId { get; set; }

    public string Title { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Requirements { get; set; }

    public string? JobType { get; set; }

    public string? Location { get; set; }

    public string? SalaryRange { get; set; }

    public DateTime PostedDate { get; set; }

    public DateTime ClosingDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public int PostedByUserId { get; set; }

    public virtual User PostedByUser { get; set; } = null!;
}
