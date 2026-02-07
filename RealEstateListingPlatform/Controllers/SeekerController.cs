using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;
using System.Security.Claims;

namespace RealEstateListingPlatform.Controllers
{
    [Authorize]
    public class SeekerController : Controller
    {
        private readonly ILeadService _leadService;

        public SeekerController(ILeadService leadService)
        {
            _leadService = leadService;
        }

        // GET: /Seeker/InterestedListings
        public async Task<IActionResult> InterestedListings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _leadService.GetMyLeadsAsSeekerAsync(userId);

            if (result.Success && result.Data != null)
            {
                var leadViewModels = result.Data.Select(l => new LeadViewModel
                {
                    Id = l.Id,
                    ListingId = l.ListingId,
                    ListingTitle = l.Listing?.Title ?? "N/A",
                    ListingAddress = $"{l.Listing?.StreetName}, {l.Listing?.Ward}, {l.Listing?.District}, {l.Listing?.City}",
                    ListingImageUrl = l.Listing?.ListingMedia?.FirstOrDefault()?.Url ?? "",
                    ListingPrice = l.Listing?.Price ?? 0,
                    SeekerName = l.Seeker?.DisplayName ?? "N/A",
                    SeekerEmail = l.Seeker?.Email ?? "N/A",
                    SeekerPhone = l.Seeker?.Phone,
                    Message = l.Message,
                    Status = l.Status ?? "New",
                    AppointmentDate = l.AppointmentDate,
                    ListerNote = l.ListerNote,
                    ListerName = l.Lister?.DisplayName ?? "N/A",
                    CreatedAt = l.CreatedAt ?? DateTime.UtcNow,
                    // Additional listing details
                    TransactionType = l.Listing?.TransactionType,
                    PropertyType = l.Listing?.PropertyType,
                    Area = l.Listing?.Area,
                    Bedrooms = l.Listing?.Bedrooms,
                    Bathrooms = l.Listing?.Bathrooms
                }).ToList();

                ViewData["Title"] = "My Interested Listings";
                return View(leadViewModels);
            }

            ViewData["Title"] = "My Interested Listings";
            return View(new List<LeadViewModel>());
        }
    }
}
