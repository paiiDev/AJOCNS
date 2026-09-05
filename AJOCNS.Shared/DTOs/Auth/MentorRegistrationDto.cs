using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Auth
{
    public class MentorRegistrationDto
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

        [Required(ErrorMessage = "Alumni GRN is required.")]
        [MaxLength(30)]
        public string AlumniGrn { get; set; }

        [Required(ErrorMessage = "Graduation year is required.")]
        [Range(1950, 2100, ErrorMessage = "Enter a valid graduation year.")]
        public short AlumniGraduationYear { get; set; }

        [MaxLength(255)]
        public string? Expertise { get; set; }
    }
}