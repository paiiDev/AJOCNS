using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Shared.DTOs.Student
{
    public class StudentFirstLoginSetupDto
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(09|\+959)\d{7,9}$", ErrorMessage = "Invalid Myanmar phone number. (e.g., 09xxxxxxxxx)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Father's name is required.")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
