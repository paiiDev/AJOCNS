using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Shared.DTOs.StudentRegistration
{
    public class AcademicYearDto
    {
        public int AcyId { get; set; }
        public string AcademicYear { get; set; } = DateTime.Now.ToString("yyyy");
        public string Status { get; set; } = string.Empty;
    }
}
