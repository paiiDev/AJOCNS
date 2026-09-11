using ajocns.database.interfaces;
using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        public AuthService(IAuthRepository authRepo, IUserRepository userRepo, IEmailService emailService)
        {
            _authRepo = authRepo;
            _userRepo = userRepo;
            _emailService = emailService;
        }

        public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetUserByEmailAsync(dto.Email);
            if (user is null || user.Status == "Inactive" || user.Status == "Pending")
            {
                return Result<AuthResultDto>.Failure("User not found, inactive, or pending approval.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Result<AuthResultDto>.Failure("Invalid credentials.");
            }

            string name = await _userRepo.GetUserNameAsync(user.UserId, user.Role);

            return Result<AuthResultDto>.Success(new AuthResultDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = name,
                Role = user.Role,
                IsFirstLogin = user.IsFirstLogin
            });
        }

        public async Task<Result<bool>> RegisterMentorAsync(MentorRegistrationDto dto)
        {
            var email = dto.Email.Trim();

            var grn = dto.AlumniGrn?.Trim();
            if (string.IsNullOrWhiteSpace(grn))
            {
                return Result<bool>.Failure("Alumni GRN is required.");
            }

            var graduationRecord = await _authRepo.GetGraduationRecordByGrnOnlyAsync(grn);
            if (graduationRecord is null)
            {
                return Result<bool>.Failure("No graduation record was found for the provided Alumni GRN. Please enter the GRN printed on your certificate.");
            }

            if (graduationRecord.GraduationYear != dto.AlumniGraduationYear)
            {
                return Result<bool>.Failure("The graduation year does not match the graduation record for this GRN.");
            }

            var existingUser = await _authRepo.GetUserByEmailForEditAsync(email);
            if (existingUser is not null)
            {
                if (existingUser.Status != "Rejected" || existingUser.Role != "Mentor" || existingUser.Mentor is null)
                {
                    return Result<bool>.Failure("An account with this email already exists.");
                }

                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                existingUser.IsFirstLogin = true;
                existingUser.Status = "Pending";
                existingUser.Mentor.Name = dto.Name.Trim();
                existingUser.Mentor.Expertise = dto.Expertise;
                existingUser.Mentor.AlumniGy = dto.AlumniGraduationYear;
                existingUser.Mentor.AlumniGrn = grn;

                bool isUpdated = await _authRepo.UpdateUserAsync(existingUser);
                if (!isUpdated)
                {
                    return Result<bool>.Failure("Failed to re-submit mentor registration. Please try again.");
                }

                return Result<bool>.Success(true);
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new User
            {
                Email = email,
                PasswordHash = hashedPassword,
                Role = "Mentor",
                Status = "Pending",
                CreatedAt = System.DateTime.UtcNow,
                IsFirstLogin = true,
                IsDeleted = false,
                Mentor = new Mentor
                {
                    Name = dto.Name.Trim(),
                    Expertise = dto.Expertise,
                    AlumniGy = dto.AlumniGraduationYear,
                    AlumniGrn = grn
                }
            };

            bool isSaved = await _authRepo.CreateUserAsync(newUser);
            if (!isSaved)
            {
                return Result<bool>.Failure("Failed to create mentor account. Please try again.");
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RegisterExternalPartnerAsync(ExternalPartnerRegistrationDto dto)
        {
            var email = dto.Email.Trim();

            var companyName = dto.CompanyName.Trim();
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return Result<bool>.Failure("Company is required.");
            }

            var existingUser = await _authRepo.GetUserByEmailForEditAsync(email);
            if (existingUser is not null)
            {
                if (existingUser.Status != "Rejected" || existingUser.Role != "ExternalPartner" || existingUser.ExternalPartner is null)
                {
                    return Result<bool>.Failure("An account with this email already exists.");
                }

                var resubmitCompany = await _authRepo.GetOrCreateCompanyAsync(companyName);
                var resubmitPosition = await _authRepo.GetOrCreatePositionAsync("Company Account");

                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                existingUser.IsFirstLogin = true;
                existingUser.Status = "Pending";
                existingUser.ExternalPartner.Name = companyName;
                existingUser.ExternalPartner.CompanyId = resubmitCompany.CompanyId;
                existingUser.ExternalPartner.PositionId = resubmitPosition.PositionId;
                existingUser.ExternalPartner.Phone = dto.Phone;

                bool isUpdated = await _authRepo.UpdateUserAsync(existingUser);
                if (!isUpdated)
                {
                    return Result<bool>.Failure("Failed to re-submit registration. Please try again.");
                }

                return Result<bool>.Success(true);
            }

            var company = await _authRepo.GetOrCreateCompanyAsync(companyName);
            var position = await _authRepo.GetOrCreatePositionAsync("Company Account");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new User
            {
                Email = email,
                PasswordHash = hashedPassword,
                Role = "ExternalPartner",
                Status = "Pending",
                CreatedAt = System.DateTime.UtcNow,
                IsFirstLogin = true,
                IsDeleted = false,
                ExternalPartner = new ExternalPartner
                {
                    Name = companyName,
                    CompanyId = company.CompanyId,
                    PositionId = position.PositionId,
                    Phone = dto.Phone
                }
            };

            bool isSaved = await _authRepo.CreateUserAsync(newUser);
            if (!isSaved)
            {
                return Result<bool>.Failure("Failed to create account. Please try again.");
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<RegisterOptionsDto>> GetRegisterOptionsAsync()
        {
            var companies = await _authRepo.GetCompaniesAsync();
            var positions = await _authRepo.GetPositionsAsync();

            var options = new RegisterOptionsDto
            {
                Companies = companies.Select(c => new CompanyOptionDto
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName
                }).ToList(),
                Positions = positions.Select(p => new PositionOptionDto
                {
                    PositionId = p.PositionId,
                    PositionName = p.Position1
                }).ToList()
            };

            return Result<RegisterOptionsDto>.Success(options);
        }

        public async Task<Result<List<PendingUserApprovalDto>>> GetPendingUsersAsync()
        {
            var users = await _authRepo.GetPendingUsersAsync();
            if (users is null || !users.Any())
            {
                return Result<List<PendingUserApprovalDto>>.Failure("No pending user approvals.");
            }

            var dtos = users.Select(u => new PendingUserApprovalDto
            {
                UserId = u.UserId,
                Email = u.Email,
                Role = u.Role,
                Name = u.Role == "Mentor"
                    ? u.Mentor?.Name ?? "-"
                    : u.Role == "ExternalPartner"
                        ? u.ExternalPartner?.Name ?? "-"
                        : "-",
                Status = u.Status,
                CreatedAt = MyanmarTime.ToMyanmar(u.CreatedAt)
            }).ToList();

            return Result<List<PendingUserApprovalDto>>.Success(dtos);
        }

        public async Task<Result<List<ExternalPartnerAdminDto>>> GetExternalPartnersAsync()
        {
            var partners = await _authRepo.GetExternalPartnersAsync();
            var dtos = partners.Select(u => new ExternalPartnerAdminDto
            {
                UserId = u.UserId,
                Name = u.ExternalPartner!.Name,
                Email = u.Email,
                CompanyName = u.ExternalPartner.Company.CompanyName,
                PositionName = u.ExternalPartner.Position.Position1,
                Phone = u.ExternalPartner.Phone,
                Expertise = u.ExternalPartner.Expertise,
                Status = u.Status,
                CreatedAt = MyanmarTime.ToMyanmar(u.CreatedAt)
            }).ToList();

            return Result<List<ExternalPartnerAdminDto>>.Success(dtos);
        }

        public async Task<Result<bool>> UpdateExternalPartnerStatusAsync(int userId, string status)
        {
            if (!await _authRepo.IsExternalPartnerAsync(userId))
            {
                return Result<bool>.Failure("External partner was not found.");
            }

            var updated = await _authRepo.UpdateUserStatusAsync(userId, status);
            return updated
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("External partner was not found or could not be updated.");
        }

        public async Task<Result<ExternalPartnerAdminDto>> GetPendingExternalPartnerAsync(int userId)
        {
            var user = await _authRepo.GetPendingUserByIdAsync(userId);
            if (user?.Role != "ExternalPartner" || user.ExternalPartner is null)
            {
                return Result<ExternalPartnerAdminDto>.Failure("Pending external partner was not found.");
            }

            return Result<ExternalPartnerAdminDto>.Success(new ExternalPartnerAdminDto
            {
                UserId = user.UserId,
                Name = user.ExternalPartner.Name,
                Email = user.Email,
                CompanyName = user.ExternalPartner.Company.CompanyName,
                PositionName = user.ExternalPartner.Position.Position1,
                Phone = user.ExternalPartner.Phone,
                Expertise = user.ExternalPartner.Expertise,
                Status = user.Status,
                CreatedAt = MyanmarTime.ToMyanmar(user.CreatedAt)
            });
        }

        public async Task<Result<bool>> ApproveUserAsync(int userId)
        {
            var user = await _authRepo.GetPendingUserByIdAsync(userId);
            if (user is null)
            {
                return Result<bool>.Failure("User not found or no longer pending approval.");
            }

            if (user.Role == "Mentor" && user.Mentor is not null)
            {
                var graduationRecord = await _authRepo.GetGraduationRecordByGrnOnlyAsync(user.Mentor.AlumniGrn);

                if (graduationRecord is null)
                {
                    return Result<bool>.Failure("No graduation record found for the provided GRN. Mentor not approved.");
                }

                if (graduationRecord.GraduationYear != user.Mentor.AlumniGy)
                {
                    return Result<bool>.Failure("The graduation year does not match the graduation record for this GRN. Mentor not approved.");
                }

                if (!string.Equals(graduationRecord.OfficialName.Trim(), user.Mentor.Name.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return Result<bool>.Failure("The name does not match the graduation record for this GRN. Mentor not approved.");
                }

                user.Mentor.Name = graduationRecord.OfficialName;
            }

            bool updated = await _authRepo.UpdateUserStatusAsync(userId, "Active");
            if (!updated)
            {
                return Result<bool>.Failure("Failed to approve user.");
            }

            string roleLabel = user.Role == "Mentor" ? "Mentor" : "External Partner";
            string body =
                $"<p>Dear {user.Mentor?.Name ?? user.ExternalPartner?.Name},</p>" +
                $"<p>Congratulations! Your account has been verified and approved as a <strong>{roleLabel}</strong> on the PUPL Alumni &amp; Career Network (AJOCNS).</p>" +
                $"<p>You can now log in with the email you registered: <strong>{user.Email}</strong>.</p>" +
                $"<p>Welcome aboard!</p>" +
                $"<p>&mdash; PUPL AJOCNS Team</p>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Your AJOCNS Account Has Been Approved", body);
            }
            catch
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RejectUserAsync(int userId)
        {
            bool updated = await _authRepo.UpdateUserStatusAsync(userId, "Rejected");
            if (!updated)
            {
                return Result<bool>.Failure("Failed to reject user.");
            }
            return Result<bool>.Success(true);
        }
    }
}