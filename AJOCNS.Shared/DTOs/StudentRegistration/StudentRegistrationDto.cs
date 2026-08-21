using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class StudentRegistrationDto
    {
        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }


        [Required(ErrorMessage = "Please select a major.")]
        public int Major_ID { get; set; }

        [Required(ErrorMessage = "Please select a programme")]
        public string GraduationStatus { get; set; } = "Undergraduate";
    }
}
