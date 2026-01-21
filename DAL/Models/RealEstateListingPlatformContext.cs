using Microsoft.EntityFrameworkCore;

namespace DAL.Models
{
    public class RealEstateListingPlatformContext : DbContext
    {
        public RealEstateListingPlatformContext(DbContextOptions<RealEstateListingPlatformContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = default!;
    }
}