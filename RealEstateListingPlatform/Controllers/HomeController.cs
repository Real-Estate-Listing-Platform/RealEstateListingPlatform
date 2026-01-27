using System.Diagnostics;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IListingService _listingService;
        public HomeController(ILogger<HomeController> logger, IListingService listingService)
        {
            _logger = logger;
            _listingService = listingService;
        }

        public async Task<IActionResult> Index()
        {
            var listings = await _listingService.GetPublishedListingsAsync();
            var properties = listings.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Address = $"{l.District}, {l.City}",
                Price = l.Price,
                Bedrooms = l.Bedrooms ?? 0,
                Bathrooms = l.Bathrooms ?? 0,
                Floors = l.Floors,
                Area = l.Area ?? "0",
                TransactionType = l.TransactionType == "Sell" ? "For Sale" : "For Rent",
                ImageUrl = l.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                   ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp"
            }).Take(6).ToList();

            return View(properties);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
