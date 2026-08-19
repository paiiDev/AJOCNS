using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Database.Interfaces
{
    public interface IUserRepository
    {
        Task<string> GetUserNameAsync(int userId, string role);
    }
}
