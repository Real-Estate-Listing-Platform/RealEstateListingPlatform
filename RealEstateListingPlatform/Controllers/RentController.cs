using System.Threading.Tasks;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Controllers
{
    public class RentController : Controller
    {
        private readonly IListingService _listingService;

        public RentController(IListingService listingService)
        {
            _listingService = listingService;
        }

        // GET: Rent
        public async Task<IActionResult> Index()
        {
            var rentals = await _listingService.GetListingsByTransactionType("Rent");
            return View(rentals);
        }
    }
}