using AJOCNS.Database.Entities;


namespace ajocns.database.interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string? email);
    }
}
