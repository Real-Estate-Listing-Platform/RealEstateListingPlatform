using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult ListingOperations()
        {
            // Mock data for UI design purposes
            var mockListings = new List<ListingApprovalViewModel>
            {
                new ListingApprovalViewModel {
                    Id = Guid.NewGuid(),
                    Title = "Luxury Villa with Pool",
                    PropertyType = "Villa",
                    Price = 500000,
                    Address = "123 District 1, HCM",
                    ListerName = "John Doe",
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new ListingApprovalViewModel {
                    Id = Guid.NewGuid(),
                    Title = "Modern Apartment near Metro",
                    PropertyType = "Apartment",
                    Price = 120000,
                    Address = "Flat 4B, District 7, HCM",
                    ListerName = "Jane Smith",
                    CreatedAt = DateTime.Now
                }
            };

            return View(mockListings);
        }

        [HttpPost]
        public IActionResult Approve(Guid id)
        {
            // Logic later
            return RedirectToAction("ListingOperations");
        }

        [HttpPost]
        public IActionResult Reject(Guid id, string reason)
        {
            // Logic later
            return RedirectToAction("ListingOperations");
        }
    }
}