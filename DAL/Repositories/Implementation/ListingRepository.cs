using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation
{
    public class ListingRepository : IListingRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public ListingRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<Listing>> GetListings()
        {
            return await _context.Listings.ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetPendingListingsAsync()
        {
            var result = await _context.Listings
                .Include(l => l.Lister).Include(nameof(Listing.ListingMedia))
                .Where(l => l.Status == "PendingReview")
                .ToListAsync();
            return result;
        }

        public async Task<Listing?> GetByIdAsync(Guid id)
        {
            return await _context.Listings.Include(l => l.Lister)
                                          .Include(nameof(Listing.ListingMedia))
                                          .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task UpdateAsync(Listing listing)
        {
            listing.UpdatedAt = DateTime.Now; 
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
        }
    }
}
