using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class EditStudentDto
    {
        public int StudentId { get; set; }

        public string Srn { get; set; } = null!;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(255)]
        public string Name { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? FatherName { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Please select a major.")]
        public int MajorId { get; set; }

        [Required(ErrorMessage = "Please select an academic year.")]
        public int AcyId { get; set; }

        [Required(ErrorMessage = "Please select a programme status.")]
        public string GraduationStatus { get; set; } = "Undergraduate";

        public bool IsGraduated { get; set; }

        public short? GraduationYear { get; set; }
    }
}
