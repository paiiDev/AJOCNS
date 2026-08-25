using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.GraduationRecords
{
    public class EditGraduationRecordDto
    {
        public int Id { get; set; }

        public int? StudentId { get; set; }

        public string? Srn { get; set; }

        [Required(ErrorMessage = "Official name is required.")]
        [StringLength(255)]
        public string OfficialName { get; set; } = null!;

        [Required(ErrorMessage = "GRN is required.")]
        [StringLength(50)]
        public string Grn { get; set; } = null!;

        [Required(ErrorMessage = "Graduation year is required.")]
        [Range(1900, 2100, ErrorMessage = "Graduation year must be a valid year.")]
        public short GraduationYear { get; set; }

        [Required(ErrorMessage = "Please select a degree.")]
        public int DegreeId { get; set; }

        [Required(ErrorMessage = "Accreditation status is required.")]
        [StringLength(50)]
        public string AccStatus { get; set; } = null!;
    }
}
