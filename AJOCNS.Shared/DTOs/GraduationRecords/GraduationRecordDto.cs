using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Shared.DTOs.GraduationRecords
{
    public class GraduationRecordDto
    {
        public int Id { get; set; }
        public int? StudentId { get; set; }
        public string? Srn { get; set; }
        public string OfficialName { get; set; }
        public string Grn { get; set; }
        public short GraduationYear { get; set; }
        public string DegreeName { get; set; }
        public string AccStatus { get; set; }
    }
}
