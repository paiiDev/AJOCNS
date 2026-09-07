namespace AJOCNS.Shared.DTOs.Mentor
{
    public class EmploymentRecordDto
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = null!;

        public int PositionId { get; set; }

        public string PositionName { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent => EndDate == null;

        public string DurationDisplay
        {
            get
            {
                var end = EndDate ?? DateTime.Now;
                var months = (end.Year - StartDate.Year) * 12 + end.Month - StartDate.Month;
                if (months < 12)
                    return $"{months} month{(months != 1 ? "s" : "")}";
                var years = months / 12;
                var remainingMonths = months % 12;
                return remainingMonths > 0 ? $"{years} year{(years != 1 ? "s" : "")} {remainingMonths} month{(remainingMonths != 1 ? "s" : "")}" : $"{years} year{(years != 1 ? "s" : "")}";
            }
        }
    }
}