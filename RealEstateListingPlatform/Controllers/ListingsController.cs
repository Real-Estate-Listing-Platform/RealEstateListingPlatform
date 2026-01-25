using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DAL.Models;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace RealEstateListingPlatform.Controllers
{
    public class ListingsController : Controller
    {
        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService)
        {
            _listingService = listingService;
        }

        // =======================
        // GET: Listings
        // =======================
        public async Task<IActionResult> Index()
        {
            var data = await _listingService.GetListings();
            return View(data);
        }

        // =======================
        // GET: Listings/Details/{id}
        // =======================
        public async Task<IActionResult> Details(Guid id)
        {
            var listing = await _listingService.GetByIdAsync(id);
            if (listing == null) return NotFound();

            return View(listing);
        }

        // =======================
        // GET: Listings/Create
        // =======================
        public IActionResult Create()
        {
            return View();
        }

        // =======================
        // POST: Listings/Create
        // =======================
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Listing listing)
        {
            ModelState.Remove("Lister");
            if (!ModelState.IsValid) return View(listing);
            listing.Id = new Guid();
            listing.CreatedAt = DateTime.UtcNow;
            listing.Status = "PendingReview";
            listing.ListerId = Guid.Parse("01DE1B1B-8B1E-43BB-9654-16E7C9CB5324");
            await _listingService.CreateAsync(listing);
            return RedirectToAction(nameof(Index));
        }

        // =======================
        // GET: Listings/Edit/{id}
        // =======================
        public async Task<IActionResult> Edit(Guid id, bool autoHide = false)
        {
            var listing = await _listingService.GetByIdAsync(id);
            if (listing == null) return NotFound();

            if (autoHide && listing.Status != "Hidden")
            {
                listing.Status = "Hidden";
                await _listingService.UpdateAsync(listing);
            }

            return View(listing);
        }

        // =======================
        // POST: Listings/Edit/{id}
        // =======================
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Listing listing)
        {
            if (id != listing.Id) return NotFound();
            id = listing.Id;
            var CreatedAt =listing.CreatedAt;
            var ListerId = listing.ListerId;
            ModelState.Remove("Lister");
            if (!ModelState.IsValid) return View(listing);

            await _listingService.UpdateAsync(listing);
            return RedirectToAction(nameof(Index));
        }

        // =======================
        // GET: Listings/Delete/{id}
        // =======================
        public async Task<IActionResult> Delete(Guid id)
        {
            var listing = await _listingService.GetByIdAsync(id);
            if (listing == null) return NotFound();

            return View(listing);
        }

        // =======================
        // POST: Listings/DeleteConfirmed/{id}
        // =======================
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _listingService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // =======================
        // GET: Listings/Pending
        // =======================
        public async Task<IActionResult> Pending()
        {
            var data = await _listingService.GetPendingListingsAsync();
            return View(data);
        }

        // =======================
        // POST: Listings/Approve/{id}
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _listingService.ApproveListingAsync(id);
            return RedirectToAction(nameof(Pending));
        }

        // =======================
        // POST: Listings/Reject/{id}
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            await _listingService.RejectListingAsync(id);
            return RedirectToAction(nameof(Pending));
        }
    }
}
