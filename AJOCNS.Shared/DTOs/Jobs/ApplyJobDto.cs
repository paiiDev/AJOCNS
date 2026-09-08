using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Jobs;

public class ApplyJobDto
{
    public int JobPostId { get; set; }
    [Required, StringLength(1000)]
    public string CoverLetter { get; set; } = string.Empty;
    [Required]
    public IFormFile? Resume { get; set; }
}
