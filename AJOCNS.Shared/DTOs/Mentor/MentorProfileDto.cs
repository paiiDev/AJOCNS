namespace AJOCNS.Shared.DTOs.Mentor
{
    public class MentorProfileDto
    {
        public int MentorId { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Expertise { get; set; }

        public short AlumniGy { get; set; }

        public string AlumniGrn { get; set; } = null!;

        public string Status { get; set; } = "";

        public List<EmploymentRecordDto> EmploymentRecords { get; set; } = new();

        public bool HasEmploymentRecords => EmploymentRecords.Any();

        public List<EmploymentRecordDto> CurrentPositions => EmploymentRecords.Where(e => e.IsCurrent).ToList();

        public List<EmploymentRecordDto> PreviousPositions => EmploymentRecords.Where(e => !e.IsCurrent).OrderByDescending(e => e.EndDate).ToList();
    }
}