using BLL.Services;
using DAL.Models;
using DAL.Repositories;
using DAL.Repositories.implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
<<<<<<< Updated upstream
        public UserService(IUserRepository userRepository) {
            _userRepository = userRepository; 
=======
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
>>>>>>> Stashed changes
        }

        public async Task<List<User>> GetUsers()
        {
            return await _userRepository.GetUsers();
        }
    }
}
