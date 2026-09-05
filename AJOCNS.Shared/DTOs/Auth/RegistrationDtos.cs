using System.Collections.Generic;

namespace AJOCNS.Shared.DTOs.Auth
{
    public class CompanyOptionDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
    }

    public class PositionOptionDto
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; }
    }

    public class RegisterOptionsDto
    {
        public List<CompanyOptionDto> Companies { get; set; } = new();
        public List<PositionOptionDto> Positions { get; set; } = new();
    }

    public class PendingUserApprovalDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}