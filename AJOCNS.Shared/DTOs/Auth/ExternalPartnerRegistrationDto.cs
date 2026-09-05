using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Auth
{
    public class ExternalPartnerRegistrationDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Company is required.")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        public int PositionId { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Expertise { get; set; }
    }
}