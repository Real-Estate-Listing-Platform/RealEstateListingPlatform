using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BLL.DTOs;
using BLL.Services;
using DAL.Models;

namespace RealEstateListingPlatform.Controllers
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class ListerController : Controller
    {
        private readonly IListingService _listingService;

        public ListerController(IListingService listingService)
        {
            _listingService = listingService;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.GetMyListingsAsync(userId);
            var listings = result.Data ?? new List<Listing>();

            var stats = new
            {
                TotalListings = listings.Count,
                ActiveListings = listings.Count(l => l.Status == "Published"),
                PendingReview = listings.Count(l => l.Status == "PendingReview"),
                TotalLeads = 47,
                NewLeads = 12,
                TotalRevenue = 125000000,
                ThisMonthRevenue = 35000000
            };

            ViewBag.Stats = stats;
            return View();
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

            return View(result.Data ?? new BLL.DTOs.PaginatedResult<Listing>());
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
        public async Task<IActionResult> Edit(Guid id, RealEstateListingPlatform.Models.ListingEditViewModel model, List<IFormFile>? mediaFiles)
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

            TempData["Success"] = "Listing updated successfully.";
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
            var customers = GetMockCustomers();
            return View(customers);
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
                new { Value = "Sale", Text = "For Sale" },
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

        private RealEstateListingPlatform.Models.ListingEditViewModel MapToEditViewModel(Listing listing)
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
                ExistingMedia = listing.ListingMedia?.ToList() ?? new List<ListingMedia>()
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
            model.ExistingMedia = listingResult.Data.ListingMedia?.ToList() ?? new List<ListingMedia>();
        }

        // Mock Data Methods (for Customers only)
        private List<MockCustomer> GetMockCustomers()
        {
            return new List<MockCustomer>
            {
                new MockCustomer
                {
                    Id = Guid.NewGuid(),
                    Name = "Nguyen Van A",
                    Email = "nguyenvana@gmail.com",
                    Phone = "0901234567",
                    InterestedProperty = "Luxury Apartment Vinhomes Central Park",
                    Status = "New",
                    Message = "I'm interested in viewing this property. When can we schedule a visit?",
                    ContactedAt = DateTime.Now.AddHours(-2)
                },
                new MockCustomer
                {
                    Id = Guid.NewGuid(),
                    Name = "Tran Thi B",
                    Email = "tranthib@outlook.com",
                    Phone = "0912345678",
                    InterestedProperty = "Studio Apartment for Rent",
                    Status = "Contacted",
                    Message = "What is included in the rental price?",
                    ContactedAt = DateTime.Now.AddDays(-1)
                },
                new MockCustomer
                {
                    Id = Guid.NewGuid(),
                    Name = "Le Minh C",
                    Email = "leminhc@yahoo.com",
                    Phone = "0923456789",
                    InterestedProperty = "Modern Villa Thu Duc",
                    Status = "Contacted",
                    Message = "Can you provide more photos of the interior?",
                    ContactedAt = DateTime.Now.AddDays(-3)
                },
                new MockCustomer
                {
                    Id = Guid.NewGuid(),
                    Name = "Pham Thi D",
                    Email = "phamthid@gmail.com",
                    Phone = "0934567890",
                    InterestedProperty = "2BR Apartment Near BTS",
                    Status = "Closed",
                    Message = "Thank you, I've rented the apartment.",
                    ContactedAt = DateTime.Now.AddDays(-7)
                },
                new MockCustomer
                {
                    Id = Guid.NewGuid(),
                    Name = "Hoang Van E",
                    Email = "hoangvane@fpt.edu.vn",
                    Phone = "0945678901",
                    InterestedProperty = "Luxury Apartment Vinhomes Central Park",
                    Status = "New",
                    Message = "Is the price negotiable?",
                    ContactedAt = DateTime.Now.AddMinutes(-30)
                }
            };
        }
    }

    // Mock Data Models (for Customers only)
    public class MockCustomer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string InterestedProperty { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ContactedAt { get; set; }
    }
}
