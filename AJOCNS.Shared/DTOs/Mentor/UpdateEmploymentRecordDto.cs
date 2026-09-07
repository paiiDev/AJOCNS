using System.ComponentModel.DataAnnotations;

namespace AJOCNS.Shared.DTOs.Mentor
{
    public class UpdateEmploymentRecordDto : CreateEmploymentRecordDto
    {
        [Required]
        public int Id { get; set; }
    }
}