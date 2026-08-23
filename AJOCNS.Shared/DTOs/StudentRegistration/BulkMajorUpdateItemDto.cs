using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class BulkMajorUpdateItemDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int MajorId { get; set; }
    }
}
