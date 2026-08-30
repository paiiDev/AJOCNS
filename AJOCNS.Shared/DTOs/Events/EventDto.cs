namespace AJOCNS.Shared.DTOs.Events
{
    public class EventDto
    {
        public int Id { get; set; }

        public int CreatedByUserId { get; set; }

        public string EventTitle { get; set; } = null!;

        public string? Description { get; set; }

        public string EventTypeName { get; set; } = null!;

        public DateTime EventDate { get; set; }

        public int? MaxCapacity { get; set; }

        public string? EventMode { get; set; }

        public string? Location { get; set; }

        public string Status { get; set; } = null!;

        public string CreatedByName { get; set; } = null!;

        public string? PosterImagePath { get; set; }
    }
}
