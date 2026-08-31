using AJOCNS.Shared.Common;
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

    }
}
