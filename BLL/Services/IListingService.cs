using DAL.Models;

namespace BLL.Services
{
    public interface IListingService
    {
        public Task<List<Listing>> GetListings();
        public Task<List<Listing>> GetListingsByTransactionType(string transactionType);
        public Task<Listing> GetListingById(Guid id);
    }
}