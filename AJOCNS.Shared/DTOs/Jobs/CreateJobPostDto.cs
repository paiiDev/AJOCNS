using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Jobs
{
    public class CreateJobPostDto
    {
        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(200)]
        public string CompanyName { get; set; } = null!;

        [Required(ErrorMessage = "Job description is required.")]
        [StringLength(5000)]
        public string Description { get; set; } = null!;

        [StringLength(5000)]
        public string? Requirements { get; set; }

        [StringLength(100)]
        public string? JobType { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? SalaryRange { get; set; }

        [Required(ErrorMessage = "Closing date is required.")]
        public DateTime ClosingDate { get; set; }
    }
}