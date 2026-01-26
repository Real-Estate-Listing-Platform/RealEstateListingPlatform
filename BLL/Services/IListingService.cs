using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IListingService
    {
        public Task<List<Listing>> GetListings();
        Task<IEnumerable<Listing>> GetPendingListingsAsync(); 
        Task<IEnumerable<Listing>> GetByTypeAsync(String type);
        Task<Listing> GetByIdAsync(Guid id);
        Task<bool> ApproveListingAsync(Guid id);
        Task<bool> RejectListingAsync(Guid id);
    }
}
