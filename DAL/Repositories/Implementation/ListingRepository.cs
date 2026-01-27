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

        // Read operations
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

        public async Task<Listing?> GetListingWithMediaAsync(Guid id)
        {
            return await _context.Listings
                .Include(l => l.ListingMedia)
                //.Include(l => l.ListingTour360)
                .Include(l => l.Lister)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<Listing>> GetListingsByListerIdAsync(Guid listerId)
        {
            return await _context.Listings
                .Where(l => l.ListerId == listerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        // Create
        public async Task<Listing> CreateAsync(Listing listing)
        {
            listing.Id = Guid.NewGuid();
            listing.CreatedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;
            listing.Status = "Draft";

            await _context.Listings.AddAsync(listing);
            await _context.SaveChangesAsync();
            return listing;
        }

        // Update
        public async Task UpdateAsync(Listing listing)
        {
            listing.UpdatedAt = DateTime.UtcNow; 
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
        }

        // Delete (Hard delete)
        public async Task<bool> DeleteAsync(Guid id)
        {
            var listing = await _context.Listings
                .Include(l => l.ListingMedia)
                //.Include(l => l.ListingPriceHistories)
                //.Include(l => l.ListingTour360)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null) return false;

            // Remove related entities first
            if (listing.ListingMedia.Any())
                _context.ListingMedia.RemoveRange(listing.ListingMedia);

            if (listing.ListingPriceHistories.Any())
                _context.ListingPriceHistories.RemoveRange(listing.ListingPriceHistories);

            if (listing.ListingTour360 != null)
                _context.ListingTour360s.Remove(listing.ListingTour360);

            // Remove the listing
            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();
            return true;
        }

        // Media Management
        public async Task AddMediaAsync(Guid listingId, ListingMedium media)
        {
            media.Id = Guid.NewGuid();
            media.ListingId = listingId;
            await _context.ListingMedia.AddAsync(media);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ListingMedium>> GetMediaByListingIdAsync(Guid listingId)
        {
            return await _context.ListingMedia
                .Where(m => m.ListingId == listingId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task DeleteMediaAsync(Guid mediaId)
        {
            var media = await _context.ListingMedia.FindAsync(mediaId);
            if (media != null)
            {
                _context.ListingMedia.Remove(media);
                await _context.SaveChangesAsync();
            }
        }

        // Validation Helpers
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Listings.AnyAsync(l => l.Id == id);
        }

        public async Task<bool> IsOwnerAsync(Guid listingId, Guid userId)
        {
            return await _context.Listings
                .AnyAsync(l => l.Id == listingId && l.ListerId == userId);
        }
    }
}
