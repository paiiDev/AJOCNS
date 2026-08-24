using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class BulkGraduationUpdateItemDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public string GraduationStatus { get; set; } = null!;
    }
}
