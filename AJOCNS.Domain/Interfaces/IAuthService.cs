using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResultDto>> LoginAsync(LoginDto dto);
        Task<Result<bool>> RegisterMentorAsync(MentorRegistrationDto dto);
        Task<Result<bool>> RegisterExternalPartnerAsync(ExternalPartnerRegistrationDto dto);
        Task<Result<RegisterOptionsDto>> GetRegisterOptionsAsync();
        Task<Result<List<PendingUserApprovalDto>>> GetPendingUsersAsync();
        Task<Result<List<ExternalPartnerAdminDto>>> GetExternalPartnersAsync();
        Task<Result<ExternalPartnerAdminDto>> GetPendingExternalPartnerAsync(int userId);
        Task<Result<bool>> UpdateExternalPartnerStatusAsync(int userId, string status);
        Task<Result<bool>> ApproveUserAsync(int userId);
        Task<Result<bool>> RejectUserAsync(int userId);
        Task<Result<ExternalPartnerAdminDto>> GetExternalPartnerProfileAsync(int userId);
        Task<Result<bool>> UpdateExternalPartnerProfileAsync(int userId, UpdateExternalPartnerProfileDto dto);
    }
}