using AJOCNS.Shared.Common;
using AJOCNS.Shared.DTOs.Student;
using AJOCNS.Shared.DTOs.StudentDashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Interfaces
{
    public interface IStudentService
    {
        Task<Result<StudentDashboardDto>> GetStudentDashboardAsync(int userId);
        Task<Result<bool>> SetupStudentFirstLoginAsync(int userId, StudentFirstLoginSetupDto dto);

    }
}
