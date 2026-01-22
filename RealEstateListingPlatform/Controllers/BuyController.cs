using System.Threading.Tasks;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Controllers
{
    public class BuyController : Controller
    {
        private readonly IListingService _listingService;

        public BuyController(IListingService listingService)
        {
            _listingService = listingService;
        }

        // GET: Buy
        public async Task<IActionResult> Index()
        {
            var sales = await _listingService.GetListingsByTransactionType("Sell");
            return View(sales);
        }
    }
}