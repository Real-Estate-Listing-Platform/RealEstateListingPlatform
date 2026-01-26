using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        public ListingService(IListingRepository listingRepository)
        {
            _listingRepository = listingRepository;
        }

        public async Task<List<Listing>> GetListings()
        {
            return await _listingRepository.GetListings();
        }

        public async Task<IEnumerable<Listing>> GetPendingListingsAsync()
        {
            var result = await _listingRepository.GetPendingListingsAsync();
            if (result == null)
            {
                return Enumerable.Empty<Listing>();
            }
            return result;
        }

        public async Task<IEnumerable<Listing>> GetByTypeAsync(String type)
        {
            var listings = await _listingRepository.GetPendingListingsAsync();
            if (listings == null)
            {
                return Enumerable.Empty<Listing>();
            }
            var filteredListings = listings.Where(l => l.TransactionType == type);
            return filteredListings;
        }

        public async Task<bool> ApproveListingAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null) return false;

            if (listing == null || listing.Status != "PendingReview")
            {
                return false;
            }

            listing.Status = "Published";
            await _listingRepository.UpdateAsync(listing);
            return true;
        }

        public async Task<bool> RejectListingAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null) return false;
            if (listing == null || listing.Status != "PendingReview")
            {
                return false;
            }
            listing.Status = "Rejected";
            await _listingRepository.UpdateAsync(listing);
            return true;
        }
    }
}
