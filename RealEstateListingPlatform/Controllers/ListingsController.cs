using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using RealEstateListingPlatform.Models;


namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : Controller
    {

        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService) => _listingService = listingService;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //return View(await _listingService.GetListings());
            return Ok(await _listingService.GetListings());
        }

<<<<<<< HEAD
        [HttpGet("/Listings/AllListings")]
        public async Task<IActionResult> AllListings([FromQuery] List<string>? propertyType = null, [FromQuery] string? location = null, [FromQuery] string? maxPrice = null)
        {
            var listings = await _listingService.GetPublishedListingsAsync();
            decimal? maxPriceNum = TryParseMaxPrice(maxPrice);

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
            }

            var viewModel = (listings?.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description ?? "N/A",
                Price = l.Price,
                PropertyType = l.PropertyType ?? "N/A",
                TransactionType = l.TransactionType == "Sell" ? "For Sale" : "For Rent",
                ListerName = l.Lister?.DisplayName ?? "Unknown User",
                Area = l.Area ?? "0",
                Address = $"{l.StreetName}, {l.Ward}, {l.District}, {l.City}",
                Bedrooms = l.Bedrooms,
                Bathrooms = l.Bathrooms,
                Floors = l.Floors,
                LegalStatus = l.LegalStatus ?? "N/A",
                FurnitureStatus = l.FurnitureStatus ?? "N/A",
                Direction = l.Direction ?? "N/A",
                CreatedAt = l.CreatedAt ?? DateTime.Now,
                ImageUrl = l.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                           ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp"
            }) ?? Enumerable.Empty<ListingApprovalViewModel>()).ToList();

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
            return View("PropertyListing", viewModel);
        }

        [HttpGet("{type}")] 
        public async Task<IActionResult> BrowseListings(string type, [FromQuery] List<string>? propertyType = null, [FromQuery] string? location = null, [FromQuery] string? maxPrice = null)
        {
            string dbType = type.Equals("Sale", StringComparison.OrdinalIgnoreCase) ? "Sell" : "Rent";
            var listings = await _listingService.GetPublishedByTypeAsync(dbType);
            decimal? maxPriceNum = TryParseMaxPrice(maxPrice);

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
            }

            var viewModel = (listings?.Select(l => new ListingApprovalViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description ?? "N/A",
                Price = l.Price,
                PropertyType = l.PropertyType ?? "N/A",
                TransactionType = l.TransactionType ?? dbType,
                ListerName = l.Lister?.DisplayName ?? "Unknown User",
                Area = l.Area ?? "0",
                Address = $"{l.StreetName}, {l.Ward}, {l.District}, {l.City}",
                Bedrooms = l.Bedrooms,
                Bathrooms = l.Bathrooms,
                Floors = l.Floors,
                LegalStatus = l.LegalStatus ?? "N/A",
                FurnitureStatus = l.FurnitureStatus ?? "N/A",
                Direction = l.Direction ?? "N/A",
                CreatedAt = l.CreatedAt ?? DateTime.Now,
                ImageUrl = l.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                           ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp"
            }) ?? Enumerable.Empty<ListingApprovalViewModel>()).ToList();

            ViewData["Title"] = type.Equals("Sale", StringComparison.OrdinalIgnoreCase) ? "Property for Sale" : "Property for Rent";
            ViewData["FilterPropertyTypes"] = propertyType ?? new List<string>();
            ViewData["FilterLocation"] = location;
            ViewData["FilterMaxPrice"] = maxPriceNum;
            ViewData["FilterMaxPriceRaw"] = maxPrice;
            ViewData["FilterFormAction"] = Url.Action("BrowseListings");
            ViewData["FilterFormType"] = type;
            return View("PropertyListing", viewModel);
        }

        [HttpGet("PropertyDetail/{id}")]
        public async Task<IActionResult> PropertyDetail(Guid id)
        {
            var property = await _listingService.GetByIdAsync(id);

            if (property == null)
            {
                return NotFound(); 
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
                ListerName = property.Lister?.DisplayName ?? "Unknown User",
                Area = property.Area ?? "0",
                Address = $"{property.StreetName}, {property.Ward}, {property.District}, {property.City}",
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

        public IActionResult ForSale()
        {
            var properties = GetMockProperties().Where(p => p.Status == "For Sale").ToList();
            ViewData["Title"] = "Nhà đất bán";
            return View("PropertyListing", properties);
        }

        public IActionResult ForRent()
        {
            var properties = GetMockProperties().Where(p => p.Status == "For Rent").ToList();
            ViewData["Title"] = "Nhà đất cho thuê";
            return View("PropertyListing", properties);
        }

        public IActionResult PropertyDetail(int id)
        {

            var property = GetMockProperties().FirstOrDefault(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            ViewData["Title"] = property.Title ?? "Property";
            return View(property);
        }

        private static List<PropertyViewModel> GetMockProperties()
        {
            return new List<PropertyViewModel>
            {
                new PropertyViewModel {
                    Id = 1,
                    Title = "Luxury Apartment with River View",
                    Location = "District 2, Ho Chi Minh City",
                    Price = 25000000000,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Area = 75, Status = "For Sale",
                    ImageUrl = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=400&q=80"
                },

                new PropertyViewModel {
                    Id = 2,
                    Title = "Modern Villa with Private Pool",
                    Location = "Thao Dien, District 2",
                    Price = 12000000000,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Area = 350, Status = "For Sale",
                    ImageUrl = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?auto=format&fit=crop&w=400&q=80"
                },

                new PropertyViewModel {
                    Id = 3,
                    Title = "Cozy Studio near Metro",
                    Location = "Binh Thanh District",
                    Price = 850000000,
                    Bedrooms = 1,
                    Bathrooms = 1,
                    Area = 45, Status = "For Sale",
                    ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=400&q=80"
                },
                new PropertyViewModel {
                    Id = 4,
                    Title = "Penthouse Sky Garden with Infinity Pool",
                    Location = "District 7, Ho Chi Minh City",
                    Price = 45000000000,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Area = 450, Status = "For Sale",
                    ImageUrl = "https://images.unsplash.com/photo-1512918728675-ed5a9ecdebfd?auto=format&fit=crop&w=800&q=80"
                },

                new PropertyViewModel {
                    Id = 5,
                    Title = "Shophouse Vinhome Central Park",
                    Location = "Binh Thanh District, HCM",
                    Price = 18500000000,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Area = 120, Status = "For Sale",
                    ImageUrl = "https://images.unsplash.com/photo-1582407947304-fd86f028f716?auto=format&fit=crop&w=800&q=80"
                },

                new PropertyViewModel {
                    Id = 6,
                    Title = "Green Garden Villa - Eco Village",
                    Location = "Thu Duc City, Ho Chi Minh",
                    Price = 28000000000,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Area = 280, Status = "For Sale",
                    ImageUrl = "https://res.cloudinary.com/dw4e01qx8/f_auto,q_auto/images/scgcqaofgcyewluey2xi"
                },

                new PropertyViewModel {
                    Id = 7, Title = "Apartment Studio Vinhomes Grand Park",
                    Location = "District 9, Ho Chi Minh", Price = 7000000,
                    Bedrooms = 1, Bathrooms = 1, Area = 35, Status = "For Rent",
                    ImageUrl = "https://images.ctfassets.net/pg6xj64qk0kh/2r4QaBLvhQFH1mPGljSdR9/39b737d93854060282f6b4a9b9893202/camden-paces-apartments-buckhead-ga-terraces-living-room-with-den_1.jpg"
                },
                new PropertyViewModel {
                    Id = 8, Title = "Office High Level - Bitexco Tower",
                    Location = "District 1, Ho Chi Minh", Price = 120000000,
                    Bedrooms = 0, Bathrooms = 2, Area = 150, Status = "For Rent",
                    ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=800&q=80"
                }
            };
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
