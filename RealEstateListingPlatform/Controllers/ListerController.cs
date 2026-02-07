using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BLL.DTOs;
using BLL.Services;

namespace RealEstateListingPlatform.Controllers
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class ListerController : Controller
    {
        private readonly IListingService _listingService;
        private readonly IPackageService _packageService;
        private readonly ILeadService _leadService;

        public ListerController(IListingService listingService, IPackageService packageService, ILeadService leadService)
        {
            _listingService = listingService;
            _packageService = packageService;
            _leadService = leadService;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            
            // Fetch comprehensive dashboard statistics
            var statsResult = await _leadService.GetDashboardStatsAsync(userId);
            
            if (!statsResult.Success || statsResult.Data == null)
            {
                // Fallback to basic stats if service fails
                var result = await _listingService.GetMyListingsAsync(userId);
                var listings = result.Data ?? new List<ListingDto>();

                var basicStats = new DashboardStatsDto
                {
                    TotalListings = listings.Count,
                    ActiveListings = listings.Count(l => l.Status == "Published"),
                    PendingReview = listings.Count(l => l.Status == "PendingReview"),
                    DraftListings = listings.Count(l => l.Status == "Draft"),
                    ExpiredListings = listings.Count(l => l.Status == "Expired"),
                    RejectedListings = listings.Count(l => l.Status == "Rejected"),
                    TotalLeads = 0,
                    NewLeads = 0,
                    ContactedLeads = 0,
                    ClosedLeads = 0,
                    ConversionRate = 0.0,
                    TotalViews = 0,
                    AverageViewsPerListing = 0.0,
                    BoostedListings = listings.Count(l => l.IsBoosted),
                    ExpiringListingsSoon = 0,
                    PublishSuccessRate = 0.0,
                    LastLeadReceivedAt = null
                };

                return View(basicStats);
            }

            return View(statsResult.Data);
        }

        // Listings Management
        public async Task<IActionResult> Listings(
            string? searchTerm,
            string? status,
            string? transactionType,
            string? propertyType,
            string? city,
            string? district,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy = "CreatedAt",
            string sortOrder = "desc",
            int pageNumber = 1,
            int pageSize = 10)
        {
            var userId = GetCurrentUserId();

            var filterParams = new BLL.DTOs.ListingFilterParameters
            {
                SearchTerm = searchTerm,
                Status = status,
                TransactionType = transactionType,
                PropertyType = propertyType,
                City = city,
                District = district,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _listingService.GetMyListingsFilteredAsync(userId, filterParams);
            
            if (TempData["Success"] != null)
            {
                ViewBag.SuccessMessage = TempData["Success"];
            }
            if (TempData["Error"] != null)
            {
                ViewBag.ErrorMessage = TempData["Error"];
            }
            if (TempData["Warning"] != null)
            {
                ViewBag.WarningMessage = TempData["Warning"];
            }

            // Pass filter parameters to view for maintaining state
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentTransactionType = transactionType;
            ViewBag.CurrentPropertyType = propertyType;
            ViewBag.CurrentCity = city;
            ViewBag.CurrentDistrict = district;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;

            // Get active boost packages for the user
            var activePackagesResult = await _packageService.GetActiveUserPackagesAsync(userId);
            var boostPackages = activePackagesResult.Success && activePackagesResult.Data != null
                ? activePackagesResult.Data.Where(p => p.Package.PackageType == "BOOST_LISTING" && p.Status == "Active").ToList()
                : new List<UserPackageDto>();
            ViewBag.BoostPackages = boostPackages;

            return View(result.Data ?? new BLL.DTOs.PaginatedResult<ListingDto>());
        }

        // GET: Lister/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = GetCurrentUserId();
            
            // Verify ownership
            if (!await _listingService.CanUserModifyListingAsync(id, userId))
            {
                TempData["Error"] = "You are not authorized to view this listing.";
                return RedirectToAction("Listings");
            }

            var result = await _listingService.GetListingWithMediaAsync(id);
            
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Listing not found.";
                return RedirectToAction("Listings");
            }

            // Get view statistics
            var viewStatsResult = await _listingService.GetListingViewStatsAsync(id, 30);
            if (viewStatsResult.Success && viewStatsResult.Data != null)
            {
                ViewBag.ViewStats = viewStatsResult.Data;
            }

            return View(result.Data);
        }

        // GET: Lister/Create
        public IActionResult Create()
        {
            PopulateDropDowns();
            return View(new RealEstateListingPlatform.Models.ListingCreateViewModel());
        }

        // GET: Lister/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = GetCurrentUserId();
            
            if (!await _listingService.CanUserModifyListingAsync(id, userId))
            {
                TempData["Error"] = "You are not authorized to edit this listing.";
                return RedirectToAction("Listings");
            }

            var result = await _listingService.GetListingWithMediaAsync(id);
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Listing not found.";
                return RedirectToAction("Listings");
            }

            // Load available packages for this user
            var activePackagesResult = await _packageService.GetActiveUserPackagesAsync(userId);
            var availablePackages = activePackagesResult.Success && activePackagesResult.Data != null
                ? activePackagesResult.Data
                    .Where(p => p.Status == "Active" && 
                           (p.Package.PackageType == "PHOTO_PACK" || p.Package.PackageType == "VIDEO_UPLOAD"))
                    .ToList()
                : new List<BLL.DTOs.UserPackageDto>();
            
            ViewBag.AvailablePackages = availablePackages;

            PopulateDropDowns();
            return View(MapToEditViewModel(result.Data));
        }

        // POST: Lister/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RealEstateListingPlatform.Models.ListingCreateViewModel model, List<IFormFile>? mediaFiles, string action)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return View(model);
            }

            var userId = GetCurrentUserId();
            var dto = MapToCreateDto(model);
            var result = await _listingService.CreateListingAsync(dto, userId, mediaFiles);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to create listing");
                PopulateDropDowns();
                return View(model);
            }

            // Check if user wants to submit for review
            if (action == "submit")
            {
                var submitResult = await _listingService.SubmitForReviewAsync(result.Data!.Id, userId);
                if (submitResult.Success)
                {
                    TempData["Success"] = "Listing created and submitted for review successfully.";
                    return RedirectToAction("Listings");
                }
                else
                {
                    TempData["Warning"] = "Listing created as draft, but submission failed: " + submitResult.Message;
                    return RedirectToAction("Edit", new { id = result.Data!.Id });
                }
            }

            // Default: save as draft
            TempData["Success"] = "Listing created as draft successfully.";
            return RedirectToAction("Edit", new { id = result.Data!.Id });
        }

        // POST: Lister/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RealEstateListingPlatform.Models.ListingEditViewModel model, List<IFormFile>? mediaFiles, Guid? photoPackageId, Guid? videoPackageId)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEditMetadataAsync(id, model);
                PopulateDropDowns();
                return View(model);
            }

            var userId = GetCurrentUserId();
            var dto = MapToUpdateDto(model);
            var result = await _listingService.UpdateListingAsync(id, dto, userId, mediaFiles);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to update listing");
                await PopulateEditMetadataAsync(id, model);
                PopulateDropDowns();
                return View(model);
            }

            // Apply packages if selected
            var packageMessages = new List<string>();
            
            if (photoPackageId.HasValue && photoPackageId.Value != Guid.Empty)
            {
                var photoPackageDto = new BLL.DTOs.ApplyPackageDto
                {
                    ListingId = id,
                    UserPackageId = photoPackageId.Value
                };
                
                var photoResult = await _packageService.ApplyPackageToListingAsync(userId, photoPackageDto);
                if (photoResult.Success)
                {
                    packageMessages.Add("Photo package applied successfully!");
                }
                else
                {
                    packageMessages.Add($"Photo package failed: {photoResult.Message}");
                }
            }
            
            if (videoPackageId.HasValue && videoPackageId.Value != Guid.Empty)
            {
                var videoPackageDto = new BLL.DTOs.ApplyPackageDto
                {
                    ListingId = id,
                    UserPackageId = videoPackageId.Value
                };
                
                var videoResult = await _packageService.ApplyPackageToListingAsync(userId, videoPackageDto);
                if (videoResult.Success)
                {
                    packageMessages.Add("Video package applied successfully!");
                }
                else
                {
                    packageMessages.Add($"Video package failed: {videoResult.Message}");
                }
            }

            // Build success message
            var successMessage = "Listing updated successfully.";
            if (packageMessages.Any())
            {
                successMessage += " " + string.Join(" ", packageMessages);
            }

            TempData["Success"] = successMessage;
            return RedirectToAction("Edit", new { id });
        }

        // POST: Lister/SubmitForReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForReview(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.SubmitForReviewAsync(id, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Edit", new { id });
            }

            TempData["Success"] = "Listing submitted for review.";
            return RedirectToAction("Listings");
        }

        // POST: Lister/UploadMedia (AJAX endpoint)
        [HttpPost]
        public async Task<IActionResult> UploadMedia(Guid listingId, IFormFile file, string mediaType)
        {
            var userId = GetCurrentUserId();
            
            if (!await _listingService.CanUserModifyListingAsync(listingId, userId))
                return Forbid();

            var result = await _listingService.AddMediaToListingAsync(listingId, file, mediaType);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Media uploaded successfully" });
        }

        // POST: Lister/DeleteMedia (AJAX endpoint)
        [HttpPost]
        public async Task<IActionResult> DeleteMedia(Guid mediaId)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.DeleteMediaAsync(mediaId, userId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Media deleted successfully" });
        }

        // POST: Lister/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.DeleteListingAsync(id, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Listing permanently deleted.";
            }

            return RedirectToAction("Listings");
        }

        // Customers/Leads Management
        public IActionResult Customers()
        {
            // Lead data is loaded via AJAX from LeadsController API
            return View();
        }


        // POST: Lister/BoostListing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoostListing(Guid listingId, Guid? userPackageId, int boostDays = 7)
        {
            var userId = GetCurrentUserId();

            var boostDto = new BoostListingDto
            {
                ListingId = listingId,
                UserPackageId = userPackageId,
                BoostDays = boostDays
            };

            var result = await _packageService.BoostListingAsync(userId, boostDto);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Listing boosted successfully! Your listing is now at the top.";
            }

            return RedirectToAction(nameof(Listings));
        }

        // Helper Methods
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in claims");

            return Guid.Parse(userIdClaim);
        }

        private void PopulateDropDowns()
        {
            ViewData["TransactionTypes"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[]
            {
                new { Value = "Sell", Text = "For Sale" },
                new { Value = "Rent", Text = "For Rent" }
            }, "Value", "Text");

            ViewData["PropertyTypes"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[]
            {
                new { Value = "Apartment", Text = "Apartment" },
                new { Value = "House", Text = "House" },
                new { Value = "Villa", Text = "Villa" },
                new { Value = "Land", Text = "Land" },
                new { Value = "Commercial", Text = "Commercial" }
            }, "Value", "Text");

            ViewData["LegalStatuses"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[]
            {
                new { Value = "RedBook", Text = "Red Book" },
                new { Value = "PinkBook", Text = "Pink Book" },
                new { Value = "SaleContract", Text = "Sale Contract" },
                new { Value = "Waiting", Text = "Waiting for Certificate" }
            }, "Value", "Text");

            ViewData["FurnitureStatuses"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[]
            {
                new { Value = "FullyFurnished", Text = "Fully Furnished" },
                new { Value = "PartiallyFurnished", Text = "Partially Furnished" },
                new { Value = "Unfurnished", Text = "Unfurnished" }
            }, "Value", "Text");

            ViewData["Directions"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[]
            {
                new { Value = "North", Text = "North" },
                new { Value = "South", Text = "South" },
                new { Value = "East", Text = "East" },
                new { Value = "West", Text = "West" },
                new { Value = "Northeast", Text = "Northeast" },
                new { Value = "Northwest", Text = "Northwest" },
                new { Value = "Southeast", Text = "Southeast" },
                new { Value = "Southwest", Text = "Southwest" }
            }, "Value", "Text");
        }

        private RealEstateListingPlatform.Models.ListingEditViewModel MapToEditViewModel(ListingDto listing)
        {
            return new RealEstateListingPlatform.Models.ListingEditViewModel
            {
                Id = listing.Id,
                Title = listing.Title,
                Description = listing.Description,
                TransactionType = listing.TransactionType ?? string.Empty,
                PropertyType = listing.PropertyType ?? string.Empty,
                Price = listing.Price,
                StreetName = listing.StreetName,
                Ward = listing.Ward,
                District = listing.District,
                City = listing.City,
                Area = listing.Area,
                HouseNumber = listing.HouseNumber,
                Latitude = listing.Latitude,
                Longitude = listing.Longitude,
                Bedrooms = listing.Bedrooms,
                Bathrooms = listing.Bathrooms,
                Floors = listing.Floors,
                LegalStatus = listing.LegalStatus,
                FurnitureStatus = listing.FurnitureStatus,
                Direction = listing.Direction,
                Status = listing.Status,
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.UpdatedAt,
                ExistingMedia = listing.ListingMedia ?? new List<ListingMediaDto>(),
                IsBoosted = listing.IsBoosted,
                IsFreeListingorder = listing.IsFreeListingorder,
                MaxPhotos = listing.MaxPhotos,
                AllowVideo = listing.AllowVideo
            };
        }

        private ListingCreateDto MapToCreateDto(RealEstateListingPlatform.Models.ListingCreateViewModel model)
        {
            return new BLL.DTOs.ListingCreateDto
            {
                Title = model.Title,
                Description = model.Description,
                TransactionType = model.TransactionType,
                PropertyType = model.PropertyType,
                Price = model.Price,
                StreetName = model.StreetName,
                Ward = model.Ward,
                District = model.District,
                City = model.City,
                Area = model.Area,
                HouseNumber = model.HouseNumber,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                Floors = model.Floors,
                LegalStatus = model.LegalStatus,
                FurnitureStatus = model.FurnitureStatus,
                Direction = model.Direction
            };
        }

        private BLL.DTOs.ListingUpdateDto MapToUpdateDto(RealEstateListingPlatform.Models.ListingEditViewModel model)
        {
            return new BLL.DTOs.ListingUpdateDto
            {
                Title = model.Title,
                Description = model.Description,
                TransactionType = model.TransactionType,
                PropertyType = model.PropertyType,
                Price = model.Price,
                StreetName = model.StreetName,
                Ward = model.Ward,
                District = model.District,
                City = model.City,
                Area = model.Area,
                HouseNumber = model.HouseNumber,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                Floors = model.Floors,
                LegalStatus = model.LegalStatus,
                FurnitureStatus = model.FurnitureStatus,
                Direction = model.Direction
            };
        }

        private async Task PopulateEditMetadataAsync(Guid id, RealEstateListingPlatform.Models.ListingEditViewModel model)
        {
            var listingResult = await _listingService.GetListingWithMediaAsync(id);
            if (!listingResult.Success || listingResult.Data == null)
                return;

            model.Status = listingResult.Data.Status;
            model.CreatedAt = listingResult.Data.CreatedAt;
            model.UpdatedAt = listingResult.Data.UpdatedAt;
            model.ExistingMedia = listingResult.Data.ListingMedia ?? new List<ListingMediaDto>();
        }

    }
}
