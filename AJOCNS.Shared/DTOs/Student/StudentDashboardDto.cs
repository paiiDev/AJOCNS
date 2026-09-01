namespace AJOCNS.Shared.DTOs.StudentDashboard
{
    public class StudentDashboardDto
    {
        public int StudentId { get; set; }

        public string Srn { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string Major { get; set; } = null!;

        public string? AcademicYear { get; set; }

        public string GraduationStatus { get; set; } = "Undergraduate";

        public bool IsGraduated { get; set; }

        public short? GraduationYear { get; set; }
    }
}