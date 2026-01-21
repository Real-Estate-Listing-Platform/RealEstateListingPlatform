using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.implementation
{
    public class ListingRepository : IListingRepository
    {
        private readonly RealEstateListingPlatformContext _context;
        public ListingRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }
        public async Task<List<Listing>> GetListings()
        {
            return await _context.Listings.ToListAsync();
        }
    }
}
