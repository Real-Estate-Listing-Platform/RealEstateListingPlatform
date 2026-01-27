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
        Task<List<Listing>> GetListings();
        Task<IEnumerable<Listing>> GetPendingListingsAsync();
        Task<IEnumerable<Listing>> GetPublishedListingsAsync();
        Task<Listing?> GetByIdAsync(Guid id);
        Task UpdateAsync(Listing listing);
    }
}
