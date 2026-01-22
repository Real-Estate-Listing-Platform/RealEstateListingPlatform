using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;
using RealEstateListingPlatform.Services;

namespace RealEstateListingPlatform.Controllers.Api
{
    [ApiController]
    [Route("api/listings")]
    public class ListingsApiController : ControllerBase
    {
        private readonly ListingService _listingService;

        public ListingsApiController(ListingService listingService)
        {
            _listingService = listingService;
        }

        // GET: /api/listings
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var listings = await _listingService.GetAllAsync();
            return Ok(listings);
        }

        // GET: /api/listings/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listing = await _listingService.GetByIdAsync(id);
            if (listing == null)
                return NotFound();

            return Ok(listing);
        }
        // POST: /api/listings
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Listing listing)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _listingService.CreateAsync(listing);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: /api/listings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Listing listing)
        {
            if (id != listing.Id)
                return BadRequest("Id mismatch");

            var updated = await _listingService.UpdateAsync(listing);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE: /api/listings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _listingService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
