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
            if (await _authRepo.EmailExistsAsync(email))
            {
                return Result<bool>.Failure("An account with this email already exists.");
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
                    AlumniGrn = dto.AlumniGrn?.Trim()
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
            if (await _authRepo.EmailExistsAsync(email))
            {
                return Result<bool>.Failure("An account with this email already exists.");
            }

            if (!await _authRepo.CompanyExistsAsync(dto.CompanyId))
            {
                return Result<bool>.Failure("Selected company is not valid.");
            }

            if (!await _authRepo.PositionExistsAsync(dto.PositionId))
            {
                return Result<bool>.Failure("Selected position is not valid.");
            }

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
                    Name = dto.Name.Trim(),
                    CompanyId = dto.CompanyId,
                    PositionId = dto.PositionId,
                    Phone = dto.Phone,
                    Expertise = dto.Expertise
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
                CreatedAt = u.CreatedAt
            }).ToList();

            return Result<List<PendingUserApprovalDto>>.Success(dtos);
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
            bool updated = await _authRepo.UpdateUserStatusAsync(userId, "Inactive");
            if (!updated)
            {
                return Result<bool>.Failure("Failed to reject user.");
            }
            return Result<bool>.Success(true);
        }
    }
}