using BLL.Services;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    public class AdminController : Controller
    {
        private readonly IListingService _listingService;
        public AdminController(IListingService listingService) => _listingService = listingService;

        [HttpGet] 
        public async Task<IActionResult> ListingOperations()
        {
            var listings = await _listingService.GetPendingListingsAsync();

            if (listings == null)
            {
                listings = new List<Listing>();
            }

            var viewModel = listings.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Price = l.Price,
                Description = l.Description ?? "N/A",
                TransactionType = l.TransactionType ?? "N/A",
                PropertyType = l.PropertyType ?? "N/A",                
                Area = l.Area ?? "N/A",
                FurnitureStatus = l.FurnitureStatus ?? "N/A",
                Direction = l.Direction ?? "N/A",
                Bedrooms = l.Bedrooms,       
                Bathrooms = l.Bathrooms,     
                LegalStatus = l.LegalStatus ?? "N/A",
                Address = $"{l.StreetName},{l.Ward},{l.District}, {l.City}",
                CreatedAt = l.CreatedAt ?? DateTime.Now,
                ListerName = l.Lister?.DisplayName ?? "Unknown User"
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _listingService.ApproveListingAsync(id);
            if (success)
                TempData["Message"] = "Listing approved successfully.";
            else
                TempData["Error"] = "Failed to approve listing.";

            return RedirectToAction("ListingOperations");

            //if (success)
            //{
            //    return Ok(new { message = "Update success in DB" });
            //}
            //return BadRequest(new { message = "Listing not found or status invalid" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            var success = await _listingService.RejectListingAsync(id);
            if (success)
                TempData["Message"] = "Listing has been rejected.";
            else
                TempData["Error"] = "Failed to reject listing.";

            return RedirectToAction("ListingOperations");

            //if (success)
            //{
            //    return Ok(new { message = "Update success in DB" });
            //}
            //return BadRequest(new { message = "Listing not found or status invalid" });
        }
    }
}
