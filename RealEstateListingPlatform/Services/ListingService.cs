using Microsoft.EntityFrameworkCore;
using RealEstateListingPlatform.Data;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Services
{
    public class ListingService
    {
        private readonly RealEstateListingPlatformContext _context;

        public ListingService(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<Listing>> GetAllAsync()
        {
            return await _context.Listing
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Listing?> GetByIdAsync(Guid id)
        {
            return await _context.Listing
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Listing> CreateAsync(Listing listing)
        {
            listing.Id = Guid.NewGuid();
            listing.CreatedAt = DateTime.UtcNow.AddHours(7);
            listing.ListerId = Guid.Parse("2A2DDC77-9F57-4B59-85B2-34D451CD9AF7");
            Console.Write(listing);
            _context.Listing.Add(listing);
            await _context.SaveChangesAsync();
            return listing;
        }

        public async Task<bool> UpdateAsync(Listing listing)
        {
            var existing = await _context.Listing.FindAsync(listing.Id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(listing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var listing = await _context.Listing.FindAsync(id);
            if (listing == null) return false;

            _context.Listing.Remove(listing);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
