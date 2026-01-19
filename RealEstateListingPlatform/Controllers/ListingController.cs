using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateListingPlatform.Data;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Controllers
{
    public class ListingController : Controller
    {
        private readonly RealEstateListingPlatformContext _context;

        public ListingController(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        // GET: /Listing
        public async Task<IActionResult> Index()
        {
            var listings = await _context.Listing.AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View("~/Views/RealEstates/Index.cshtml", listings);
        }

        // GET: /Listing/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var listings = await _context.Listing.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id.Value);

            if (listings == null) return NotFound();

            return View("~/Views/RealEstates/Details.cshtml", listings);
        }

        // GET: /Listing/Create
        public IActionResult Create()
        {
            return View("~/Views/RealEstates/Create.cshtml");
        }

        // POST: /Listing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ListerId,Title,TransactionType,PropertyType,Price,StreetName,Area,Ward,City,HouseNumber,Latitude,Longitude,Status")] Listing listing)
        {
            if (!ModelState.IsValid) return View("~/Views/RealEstates/Create.cshtml", listing);

            listing.Id = listing.Id == Guid.Empty ? Guid.NewGuid() : listing.Id;
            listing.CreatedAt = DateTime.UtcNow.AddHours(7);

            _context.Listing.Add(listing);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Listing/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var listing = await _context.Listing.FindAsync(id.Value);
            if (listing == null) return NotFound();

            return View("~/Views/RealEstates/Edit.cshtml", listing);
        }

        // POST: /Listing/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ListerId,Title,TransactionType,PropertyType,Price,StreetName,Area,Ward,City,HouseNumber,Latitude,Longitude,Status")] Listing listing)
        {
            if (id != listing.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Listing.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.ListerId = listing.ListerId;
                    existing.Title = listing.Title;
                    existing.TransactionType = listing.TransactionType;
                    existing.PropertyType = listing.PropertyType;
                    existing.Price = listing.Price;
                    existing.StreetName = listing.StreetName;
                    existing.Area = listing.Area;
                    existing.Ward = listing.Ward;
                    existing.City = listing.City;
                    existing.HouseNumber = listing.HouseNumber;
                    existing.Latitude = listing.Latitude;
                    existing.Longitude = listing.Longitude;
                    existing.Status = listing.Status;
                    Console.Write(existing.Latitude);
                    _context.Entry(existing).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListingExists(listing.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/RealEstates/Edit.cshtml", listing);
        }

        // GET: /Listing/Delete/{id}
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var listing = await _context.Listing.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id.Value);

            if (listing == null) return NotFound();

            return View("~/Views/RealEstates/Delete.cshtml", listing);
        }

        // POST: /Listing/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var listing = await _context.Listing.FindAsync(id);
            if (listing != null)
            {
                _context.Listing.Remove(listing);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ListingExists(Guid id)
        {
            return _context.Listing.Any(e => e.Id == id);
        }
    }
}
