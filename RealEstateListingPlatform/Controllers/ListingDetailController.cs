using System;
using System.Threading.Tasks;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Controllers
{
    public class ListingDetailController : Controller
    {
        private readonly IListingService _listingService;

        public ListingDetailController(IListingService listingService)
        {
            _listingService = listingService;
        }

        // GET: ListingDetail/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var listing = await _listingService.GetListingById(id);

            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }
    }
}