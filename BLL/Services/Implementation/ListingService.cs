using DAL.Models;
using DAL.Repositories;
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
