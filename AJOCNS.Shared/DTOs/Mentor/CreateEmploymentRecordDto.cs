using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Mentor
{
    public class CreateEmploymentRecordDto
    {
        [Required]
        public string CompanyName { get; set; } = null!;

        [Required]
        public string PositionName { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrentPosition { get; set; } = false;
    }
}