using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Events
{
    public class UpdateEventDto : CreateEventDto
    {
        [Required(ErrorMessage = "Event id is required.")]
        public int Id { get; set; }

        public string? CurrentPosterPath { get; set; }
    }
}