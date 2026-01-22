using DAL.Models;
using DAL.Repositories;

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

        public async Task<List<Listing>> GetListingsByTransactionType(string transactionType)
        {
            return await _listingRepository.GetListingsByTransactionType(transactionType);
        }

        public async Task<Listing> GetListingById(Guid id)
        {
            return await _listingRepository.GetListingById(id);
        }
    }
}