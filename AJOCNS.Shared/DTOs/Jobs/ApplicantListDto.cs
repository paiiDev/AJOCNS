namespace AJOCNS.Shared.DTOs.Jobs;

public class ApplicantListDto
{
    public int ApplicationId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public string? ResumeUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}
