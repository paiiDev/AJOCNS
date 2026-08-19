using ajocns.database.interfaces;
using AJOCNS.Database.Entities;
using AJOCNS.Database.Interfaces;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IUserRepository _userRepo;
        public AuthService(IAuthRepository authRepo, IUserRepository userRepo)
        {
            _authRepo = authRepo;
            _userRepo = userRepo;
        }

        public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetUserByEmailAsync(dto.Email);
            if (user is null || user.Status == "Inactive")
            {
                return Result<AuthResultDto>.Failure("User not found or inactive.");
            }
            else
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                string name = await _userRepo.GetUserNameAsync(user.UserId, user.Role);
                if (!isPasswordValid)
                {
                    return Result<AuthResultDto>.Failure("Invalid credentials.");
                }



                return Result<AuthResultDto>.Success(new AuthResultDto 
                { UserId = user.UserId, 
                    Email = user.Email, 
                    Name = name, 
                    Role = user.Role });
                 }
        }


    }
}

