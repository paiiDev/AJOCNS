using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class StudentDto
    {


        public string Srn { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Phone { get; set; }

        public string? FatherName { get; set; }

        public string? Address { get; set; }

        public string Major { get; set; }

        public string? GraduationStatus { get; set; }


    }
}
