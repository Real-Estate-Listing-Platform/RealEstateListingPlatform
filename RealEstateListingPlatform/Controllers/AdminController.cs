using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Controllers
{
    public class AdminController : Controller
    {
        private readonly IListingService _listingService;
        public AdminController(IListingService listingService) => _listingService = listingService;

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _listingService.ApproveListingAsync(id);
            //if (success)
            //    TempData["Message"] = "Listing approved successfully.";
            //else
            //    TempData["Error"] = "Failed to approve listing.";

            //return RedirectToAction("Index");

            if (success)
            {
                return Ok(new { message = "Update success in DB" });
            }
            return BadRequest(new { message = "Listing not found or status invalid" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            var success = await _listingService.RejectListingAsync(id);
            //if (success)
            //    TempData["Message"] = "Listing has been rejected.";
            //else
            //    TempData["Error"] = "Failed to reject listing.";

            //return RedirectToAction("Index");
            if (success)
            {
                return Ok(new { message = "Update success in DB" });
            }
            return BadRequest(new { message = "Listing not found or status invalid" });
        }
    }
}
