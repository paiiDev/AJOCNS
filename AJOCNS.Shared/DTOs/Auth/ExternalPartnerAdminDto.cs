namespace AJOCNS.Shared.DTOs.Auth
{
    public class ExternalPartnerAdminDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Expertise { get; set; }
        public string Status { get; set; } = string.Empty;
        public System.DateTime CreatedAt { get; set; }
    }
}
