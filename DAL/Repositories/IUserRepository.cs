using DAL.Models;

namespace DAL.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsers();
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserById(Guid id);
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteUser(User user);
        Task<bool> UserExists(string email);
        IQueryable<User> GetUsersQueryable();
    }
}