using Microsoft.AspNetCore.Mvc;
using BLL.Services; 
using RealEstateListingPlatform.Models;
using DAL.Models;


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

        [HttpGet("{type}")] 
        public async Task<IActionResult> BrowseListings(string type)
        {
            
            string dbType = type.Equals("Sale", StringComparison.OrdinalIgnoreCase) ? "Sell" : "Rent";
            var listings = await _listingService.GetByTypeAsync(dbType);
            var viewModel = (listings ?? Enumerable.Empty<Listing>()).Select(l => new ListingApprovalViewModel
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
            }).ToList();

            ViewData["Title"] = type.Equals("Sale", StringComparison.OrdinalIgnoreCase)
                ? "Property for Sale"
                : "Property for Rent";
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
                ImageUrl = property.ListingMedia?.OrderBy(m => m.Id).Select(m => m.Url).FirstOrDefault()
                   ?? "https://tjh.com/wp-content/uploads/2023/06/TJH_HERO_TJH-HOME@2x-1.webp"
            };

            ViewData["Title"] = "Property Detail";
            return View("PropertyDetail", viewModel); 
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
