using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Controllers
{
    public class AdminController : Controller
    {
        // Trang Dashboard tổng quan
        public IActionResult Index()
        {
            return View();
        }

        // Trang danh sách bất động sản
        public IActionResult Listings()
        {
            return View();
        }
    }
}