namespace AJOCNS.Shared.DTOs.Jobs;

public class AppliedJobDto
{
    public int JobPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
