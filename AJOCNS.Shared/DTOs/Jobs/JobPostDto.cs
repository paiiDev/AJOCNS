namespace AJOCNS.Shared.DTOs.Jobs
{
    public class JobPostDto
    {
        public int Id { get; set; }

        public int PostedByUserId { get; set; }

        public string Title { get; set; } = null!;

        public string CompanyName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? Requirements { get; set; }

        public string? JobType { get; set; }

        public string? Location { get; set; }

        public string? SalaryRange { get; set; }

        public DateTime PostedDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public string Status { get; set; } = null!;

        public string PostedByName { get; set; } = null!;
    }
}