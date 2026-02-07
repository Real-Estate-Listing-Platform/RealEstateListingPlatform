using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using BLL.DTOs;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    public class ListingsController : Controller
    {

        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService) => _listingService = listingService;

        public async Task<IActionResult> Index()
        {
            return View(await _listingService.GetListings());
        }

        public async Task<IActionResult> AllListings(List<string>? propertyType = null, string? location = null, string? maxPrice = null, int page = 1, int pageSize = 12)
        {
            var listings = await _listingService.GetPublishedListingsAsync();
            decimal? maxPriceNum = TryParseMaxPrice(maxPrice);
            int totalCount = 0;

            if (listings != null)
            {
                if (propertyType != null && propertyType.Count > 0)
                {
                    var types = propertyType.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
                    if (types.Count > 0)
                        listings = listings.Where(l => types.Any(t => string.Equals(t, l.PropertyType, StringComparison.OrdinalIgnoreCase))).ToList();
                }

                if (!string.IsNullOrWhiteSpace(location))
                {
                    var loc = location.Trim();
                    listings = listings.Where(l =>
                        (l.StreetName != null && l.StreetName.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.Ward != null && l.Ward.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.District != null && l.District.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.City != null && l.City.Contains(loc, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                if (maxPriceNum.HasValue && maxPriceNum.Value > 0)
                    listings = listings.Where(l => l.Price <= maxPriceNum.Value).ToList();
                
                // Re-apply boost ordering after filtering (boosted listings first)
                listings = listings.OrderByDescending(l => l.IsBoosted)
                                 .ThenByDescending(l => l.CreatedAt)
                                 .ToList();
            }

            // Calculate pagination
            totalCount = listings?.Count() ?? 0;
            var paginatedListings = listings?.Skip((page - 1) * pageSize).Take(pageSize).ToList() ?? new List<ListingDto>();

            var viewModel = paginatedListings.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description ?? "N/A",
                Price = l.Price,
                PropertyType = l.PropertyType ?? "N/A",
                TransactionType = l.TransactionType == "Sell" ? "For Sale" : "For Rent",
                ListerName = l.ListerName ?? "Unknown User",
                Area = l.Area ?? "0",
                Address = $"{l.HouseNumber}, {l.StreetName}, {l.Ward}, {l.District}, {l.City}",
                Bedrooms = l.Bedrooms,
                Bathrooms = l.Bathrooms,
                Floors = l.Floors,
                LegalStatus = l.LegalStatus ?? "N/A",
                FurnitureStatus = l.FurnitureStatus ?? "N/A",
                Direction = l.Direction ?? "N/A",
                CreatedAt = l.CreatedAt ?? DateTime.Now,
                ImageUrl = l.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                           ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp",
                IsBoosted = l.IsBoosted
            }).ToList();

            var titleParts = new List<string>();
            if (propertyType != null && propertyType.Count > 0) titleParts.Add(string.Join(", ", propertyType));
            if (!string.IsNullOrWhiteSpace(location)) titleParts.Add($"Khu vực: {location}");
            if (maxPriceNum.HasValue) titleParts.Add($"Giá tối đa: {maxPriceNum.Value / 1_000_000_000:N0} tỷ");
            ViewData["Title"] = titleParts.Count > 0 ? $"All Listings – {string.Join(", ", titleParts)}" : "All Listings";
            ViewData["FilterPropertyTypes"] = propertyType ?? new List<string>();
            ViewData["FilterLocation"] = location;
            ViewData["FilterMaxPrice"] = maxPriceNum;
            ViewData["FilterMaxPriceRaw"] = maxPrice;
            ViewData["FilterFormAction"] = Url.Action("AllListings");
            ViewData["FilterFormType"] = (string?)null;
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalCount"] = totalCount;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            return View("PropertyListing", viewModel);
        }

        [Route("Listings/{type}")]
        public async Task<IActionResult> BrowseListings(string type, List<string>? propertyType = null, string? location = null, string? maxPrice = null, int page = 1, int pageSize = 12)
        {
            string dbType = type.Equals("Sell", StringComparison.OrdinalIgnoreCase) ? "Sell" : "Rent";
            var listings = await _listingService.GetPublishedByTypeAsync(dbType);
            decimal? maxPriceNum = TryParseMaxPrice(maxPrice);
            int totalCount = 0;

            if (listings != null)
            {
                if (propertyType != null && propertyType.Count > 0)
                {
                    var types = propertyType.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
                    if (types.Count > 0)
                        listings = listings.Where(l => types.Any(t => string.Equals(t, l.PropertyType, StringComparison.OrdinalIgnoreCase))).ToList();
                }
                if (!string.IsNullOrWhiteSpace(location))
                {
                    var loc = location.Trim();
                    listings = listings.Where(l =>
                        (l.StreetName != null && l.StreetName.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.Ward != null && l.Ward.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.District != null && l.District.Contains(loc, StringComparison.OrdinalIgnoreCase)) ||
                        (l.City != null && l.City.Contains(loc, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
                if (maxPriceNum.HasValue && maxPriceNum.Value > 0)
                    listings = listings.Where(l => l.Price <= maxPriceNum.Value).ToList();
                
                // Re-apply boost ordering after filtering (boosted listings first)
                listings = listings.OrderByDescending(l => l.IsBoosted)
                                 .ThenByDescending(l => l.CreatedAt)
                                 .ToList();
            }

            // Calculate pagination
            totalCount = listings?.Count() ?? 0;
            var paginatedListings = listings?.Skip((page - 1) * pageSize).Take(pageSize).ToList() ?? new List<ListingDto>();

            var viewModel = paginatedListings.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description ?? "N/A",
                Price = l.Price,
                PropertyType = l.PropertyType ?? "N/A",
                TransactionType = l.TransactionType ?? dbType,
                ListerName = l.ListerName ?? "Unknown User",
                Area = l.Area ?? "0",
                Address = $"{l.HouseNumber}, {l.StreetName}, {l.Ward}, {l.District}, {l.City}",
                Bedrooms = l.Bedrooms,
                Bathrooms = l.Bathrooms,
                Floors = l.Floors,
                LegalStatus = l.LegalStatus ?? "N/A",
                FurnitureStatus = l.FurnitureStatus ?? "N/A",
                Direction = l.Direction ?? "N/A",
                CreatedAt = l.CreatedAt ?? DateTime.Now,
                ImageUrl = l.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                           ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp",
                IsBoosted = l.IsBoosted
            }).ToList();

            ViewData["Title"] = type.Equals("Sell", StringComparison.OrdinalIgnoreCase) ? "Property for Sale" : "Property for Rent";
            ViewData["FilterPropertyTypes"] = propertyType ?? new List<string>();
            ViewData["FilterLocation"] = location;
            ViewData["FilterMaxPrice"] = maxPriceNum;
            ViewData["FilterMaxPriceRaw"] = maxPrice;
            ViewData["FilterFormAction"] = Url.Action("BrowseListings");
            ViewData["FilterFormType"] = type;
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalCount"] = totalCount;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            return View("PropertyListing", viewModel);
        }

        [Route("Listings/PropertyDetail/{id}")]
        public async Task<IActionResult> PropertyDetail(Guid id)
        {
            var property = await _listingService.GetByIdAsync(id);

            if (property == null)
            {
                return NotFound(); 
            }

            // Track the view (properly await to prevent DbContext disposal issues)
            try
            {
                var userId = User.Identity?.IsAuthenticated == true 
                    ? Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString())
                    : (Guid?)null;
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                
                await _listingService.TrackViewAsync(id, userId, ipAddress, userAgent);
            }
            catch (Exception)
            {
                // Silently ignore view tracking errors - don't break the page
            }
   
            var mediaUrls = property.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url ?? string.Empty).ToList() ?? new List<string>();
            var defaultImg = "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp";
            if (mediaUrls.Count == 0) mediaUrls.Add(defaultImg);

            var viewModel = new ListingApprovalViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description ?? "N/A",
                Price = property.Price,
                PropertyType = property.PropertyType ?? "N/A",
                TransactionType = property.TransactionType ?? "N/A",
                ListerName = property.ListerName ?? "Unknown User",
                Area = property.Area ?? "0",
                Address = $"{property.HouseNumber}, {property.StreetName}, {property.Ward}, {property.District}, {property.City}",
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Floors = property.Floors,
                LegalStatus = property.LegalStatus ?? "N/A",
                FurnitureStatus = property.FurnitureStatus ?? "N/A",
                Direction = property.Direction ?? "N/A",
                CreatedAt = property.CreatedAt ?? DateTime.Now,
                ImageUrl = mediaUrls.First(),
                ImageUrls = mediaUrls
            };

            ViewData["Title"] = "Property Detail";
            return View("PropertyDetail", viewModel); 
        }

        private static decimal? TryParseMaxPrice(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var t = s.Trim();
            if (decimal.TryParse(t.Replace(",", "").Replace(" ", ""), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v < 1_000_000 ? v * 1_000_000_000 : v;
            var m = System.Text.RegularExpressions.Regex.Match(t, @"(\d+(?:[.,]\d+)?)\s*(tỷ|ty|B|b)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success && decimal.TryParse(m.Groups[1].Value.Replace(",", "."), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var num))
                return string.IsNullOrEmpty(m.Groups[2].Value) ? (num < 1_000_000 ? num * 1_000_000_000 : num) : num * 1_000_000_000;
            return null;
        }

        //// GET: Listings/Details/5
        //public async Task<IActionResult> Details(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var listing = await _context.Listings
        //        .Include(l => l.Lister)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (listing == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(listing);
        //}

        //// GET: Listings/Create
        //public IActionResult Create()
        //{
        //    ViewData["ListerId"] = new SelectList(_context.Users, "Id", "DisplayName");
        //    return View();
        //}

        //// POST: Listings/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,ListerId,Title,Description,TransactionType,PropertyType,Price,StreetName,Ward,District,City,Area,HouseNumber,Latitude,Longitude,Bedrooms,Bathrooms,Floors,LegalStatus,FurnitureStatus,Direction,Status,ExpirationDate,CreatedAt,UpdatedAt")] Listing listing)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        listing.Id = Guid.NewGuid();
        //        _context.Add(listing);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ListerId"] = new SelectList(_context.Users, "Id", "DisplayName", listing.ListerId);
        //    return View(listing);
        //}

        //// GET: Listings/Edit/5
        //public async Task<IActionResult> Edit(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var listing = await _context.Listings.FindAsync(id);
        //    if (listing == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["ListerId"] = new SelectList(_context.Users, "Id", "DisplayName", listing.ListerId);
        //    return View(listing);
        //}

        //// POST: Listings/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Guid id, [Bind("Id,ListerId,Title,Description,TransactionType,PropertyType,Price,StreetName,Ward,District,City,Area,HouseNumber,Latitude,Longitude,Bedrooms,Bathrooms,Floors,LegalStatus,FurnitureStatus,Direction,Status,ExpirationDate,CreatedAt,UpdatedAt")] Listing listing)
        //{
        //    if (id != listing.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(listing);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ListingExists(listing.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ListerId"] = new SelectList(_context.Users, "Id", "DisplayName", listing.ListerId);
        //    return View(listing);
        //}

        //// GET: Listings/Delete/5
        //public async Task<IActionResult> Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var listing = await _context.Listings
        //        .Include(l => l.Lister)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (listing == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(listing);
        //}

        //// POST: Listings/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(Guid id)
        //{
        //    var listing = await _context.Listings.FindAsync(id);
        //    if (listing != null)
        //    {
        //        _context.Listings.Remove(listing);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool ListingExists(Guid id)
        //{
        //    return _context.Listings.Any(e => e.Id == id);
        //}
    }
}
