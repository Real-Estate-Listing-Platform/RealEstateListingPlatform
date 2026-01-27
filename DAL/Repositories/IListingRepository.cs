using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IListingRepository
    {
        // Read operations
        Task<List<Listing>> GetListings();
        Task<IEnumerable<Listing>> GetPendingListingsAsync();
        Task<Listing?> GetByIdAsync(Guid id);
        Task<Listing?> GetListingWithMediaAsync(Guid id);
        Task<List<Listing>> GetListingsByListerIdAsync(Guid listerId);
        
        // Create
        Task<Listing> CreateAsync(Listing listing);
        
        // Update
        Task UpdateAsync(Listing listing);
        
        // Delete (Hard delete)
        Task<bool> DeleteAsync(Guid id);
        
        // Media Management
        Task AddMediaAsync(Guid listingId, ListingMedium media);
        Task<List<ListingMedium>> GetMediaByListingIdAsync(Guid listingId);
        Task DeleteMediaAsync(Guid mediaId);
        
        // Validation Helpers
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsOwnerAsync(Guid listingId, Guid userId);
    }
}
