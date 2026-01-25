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

        public async Task<Listing?> GetByIdAsync(Guid id)
        {
            return await _listingRepository.GetByIdAsync(id);
        }

        public async Task<Listing> CreateAsync(Listing listing)
        {
            listing.Id = Guid.NewGuid();
            listing.CreatedAt = DateTime.UtcNow.AddHours(7);
            listing.ListerId = Guid.Parse("01DE1B1B-8B1E-43BB-9654-16E7C9CB5324");

            await _listingRepository.AddAsync(listing);
            return listing;
        }

        public async Task<bool> UpdateAsync(Listing listing)
        {
            return await _listingRepository.UpdateAsync(listing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _listingRepository.DeleteAsync(id);
        }
    }
}
