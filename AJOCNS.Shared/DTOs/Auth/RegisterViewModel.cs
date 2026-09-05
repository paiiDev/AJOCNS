using System.Collections.Generic;

namespace AJOCNS.Shared.DTOs.Auth
{
    public class RegisterViewModel
    {
        public MentorRegistrationDto Mentor { get; set; } = new();
        public ExternalPartnerRegistrationDto ExternalPartner { get; set; } = new();
        public RegisterOptionsDto Options { get; set; } = new();
    }
}