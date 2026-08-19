using AJOCNS.Database.Entities;
using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResultDto>> LoginAsync(LoginDto dto);
    }
}
