using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Jobs
{
    public class UpdateJobPostDto : CreateJobPostDto
    {
        [Required(ErrorMessage = "Job post id is required.")]
        public int Id { get; set; }
    }
}