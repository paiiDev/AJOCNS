using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Events
{
    public class SendZoomLinkDto
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = null!;

        [Required(ErrorMessage = "Please provide a Zoom link.")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string ZoomLink { get; set; } = null!;
    }
}
