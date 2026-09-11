using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Auth
{
    public class UpdateExternalPartnerProfileDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company is required.")]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Expertise { get; set; }
    }
}