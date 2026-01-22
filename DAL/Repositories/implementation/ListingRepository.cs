using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.implementation
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
            return await _context.Listings
                .Include(l => l.Lister)
                .Include(l => l.ListingMedia)
                .ToListAsync();
        }

        public async Task<List<Listing>> GetListingsByTransactionType(string transactionType)
        {
            return await _context.Listings
                .Include(l => l.Lister)
                .Include(l => l.ListingMedia)
                .Where(l => l.TransactionType == transactionType && l.Status == "Published")
                .ToListAsync();
        }

        public async Task<Listing> GetListingById(Guid id)
        {
            return await _context.Listings
                .Include(l => l.Lister)
                .Include(l => l.ListingMedia)
                .Include(l => l.ListingTour360)
                .Include(l => l.ListingPriceHistories)
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}