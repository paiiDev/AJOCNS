using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Events
{
    public class CreateEventDto
    {
        [Required(ErrorMessage = "Event title is required.")]
        [StringLength(255)]
        public string EventTitle { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select an event type.")]
        public int EventTypeId { get; set; }

        [Required(ErrorMessage = "Event date is required.")]
        public DateTime EventDate { get; set; }

        [Range(1, 100000, ErrorMessage = "Max capacity must be at least 1.")]
        public int? MaxCapacity { get; set; }

        [StringLength(50)]
        public string? EventMode { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        public IFormFile? PosterImage { get; set; }
    }
}
