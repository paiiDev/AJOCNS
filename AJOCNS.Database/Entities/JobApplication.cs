namespace AJOCNS.Database.Entities;

public partial class JobApplication
{
    public int JobApplicationId { get; set; }
    public int JobPostId { get; set; }
    public int StudentId { get; set; }
    public string CoverLetter { get; set; } = null!;
    public string? ResumeUrl { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = null!;
    public virtual JobPost JobPost { get; set; } = null!;
    public virtual User StudentUser { get; set; } = null!;
}
