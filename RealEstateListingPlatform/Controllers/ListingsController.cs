using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL.Models;
using BLL.Services;

namespace RealEstateListingPlatform.Controllers
{
    public class ListingsController : Controller
    {
        private readonly IListingService _listingService;
        public ListingsController(IListingService listingService)
        {
            this._listingService = listingService;
        }

        // GET: Listings
        public async Task<IActionResult> Index()
        {
            return View(await _listingService.GetListings());
        }

        public async Task<IActionResult> PendingListings()
        {
            var listings = await _listingService.GetPendingListingsAsync();
            return View(listings);
        }

        public async Task<IActionResult> FilterByType(string type)
        {
            var listings = await _listingService.GetByTypeAsync(type);
            return View("PendingListings", listings);
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
