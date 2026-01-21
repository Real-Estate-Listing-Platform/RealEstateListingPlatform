using DAL.Models;

namespace BLL.Services
{
    public interface IUserService
    {
        public Task<List<User>> GetUsers();
    }
}
