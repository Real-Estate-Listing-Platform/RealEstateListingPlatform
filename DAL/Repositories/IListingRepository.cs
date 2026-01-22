using DAL.Models;

namespace DAL.Repositories
{
    public interface IListingRepository
    {
        Task<List<Listing>> GetListings();
        Task<List<Listing>> GetListingsByTransactionType(string transactionType);
        Task<Listing> GetListingById(Guid id);
    }
}