using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Mentor
{
    public class MentorProfileEditDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Expertise { get; set; }
    }
}