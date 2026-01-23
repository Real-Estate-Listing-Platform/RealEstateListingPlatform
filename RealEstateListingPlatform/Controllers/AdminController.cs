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
            var listings = _listingService.GetPendingListingsAsync().Result ?? new List<Listing>(); ;
            
            var viewModel = listings.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Price = l.Price,                
                PropertyType = l.PropertyType ?? "N/A",
                Address = $"{l.District}, {l.City}",
                CreatedAt = l.CreatedAt ?? DateTime.Now,
                ListerName = "Owner"
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
