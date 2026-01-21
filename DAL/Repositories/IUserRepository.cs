using DAL.Models;

namespace DAL.Repositories
{
    public interface IUserRepository
    {
        public Task<List<User>> GetUsers();
    }
}
