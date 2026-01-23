using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public UserRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
