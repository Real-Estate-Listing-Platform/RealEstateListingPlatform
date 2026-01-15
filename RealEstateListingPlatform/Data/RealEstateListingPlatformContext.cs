using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Data
{
    public class RealEstateListingPlatformContext : DbContext
    {
        public RealEstateListingPlatformContext (DbContextOptions<RealEstateListingPlatformContext> options)
            : base(options)
        {
        }

        public DbSet<RealEstateListingPlatform.Models.Test> Test { get; set; } = default!;
    }
}
