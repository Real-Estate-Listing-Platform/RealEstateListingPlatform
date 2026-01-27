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
        public DbSet<Listing> Listings { get; set; } = default!;
        public DbSet<ListingMedia> ListingMedia { get; set; } = default!;
        public DbSet<ListingPriceHistory> ListingPriceHistories { get; set; } = default!;
        public DbSet<ListingTour360> ListingTour360s { get; set; } = default!;
        public DbSet<Favorite> Favorites { get; set; } = default!;
        public DbSet<Lead> Leads { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<Report> Reports { get; set; } = default!;
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
        
        // Payment and Package system
        public DbSet<ListingPackage> ListingPackages { get; set; } = default!;
        public DbSet<UserPackage> UserPackages { get; set; } = default!;
        public DbSet<Transaction> Transactions { get; set; } = default!;
        public DbSet<ListingBoost> ListingBoosts { get; set; } = default!;
    }
}