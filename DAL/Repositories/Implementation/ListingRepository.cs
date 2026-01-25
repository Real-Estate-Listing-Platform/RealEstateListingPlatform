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
                .Include(l => l.Lister)
                .Where(l => l.Status == "PendingReview")
                .ToListAsync();
            return result;
        }

        public async Task<Listing?> GetByIdAsync(Guid id)
        {
            return await _context.Listings.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(Listing listing)
        {
            var existing = await _context.Listings.FindAsync(listing.Id);
            listing.UpdatedAt = DateTime.UtcNow;
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(listing);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task AddAsync(Listing listing)
        {
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null) return false;

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
