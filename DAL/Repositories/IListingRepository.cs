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
        Task<Listing?> GetByIdAsync(Guid id);
        Task AddAsync(Listing listing);
        Task<bool> UpdateAsync(Listing listing);
        Task<bool> DeleteAsync(Guid id);
    }
}
