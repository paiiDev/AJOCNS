using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class BulkGraduationUpdateRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "No updates provided.")]
        public List<BulkGraduationUpdateItemDto> Updates { get; set; } = new();

        [Required(ErrorMessage = "Graduation year is required.")]
        [Range(1900, 2100, ErrorMessage = "Graduation year must be a valid year.")]
        public short GraduationYear { get; set; }
    }
}
