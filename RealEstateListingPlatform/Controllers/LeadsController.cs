using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateListingPlatform.Models;
using System.Security.Claims;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadsController : ControllerBase
    {
        private readonly ILeadService _leadService;

        public LeadsController(ILeadService leadService)
        {
            _leadService = leadService;
        }

        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> CreateLead([FromBody] CreateLeadDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            DateTime? appointmentDate = null;
            if (dto.AppointmentDate.HasValue)
            {
                appointmentDate = dto.AppointmentDate.Value;
            }

            var result = await _leadService.CreateLeadAsync(dto.ListingId, userId, dto.Message, appointmentDate);

            if (result.Success)
            {
                // Return only the lead ID, not the full entity with navigation properties
                return Ok(new { success = true, message = result.Message, data = new { id = result.Data?.Id } });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpGet("MyLeadsAsLister")]
        [Authorize]
        public async Task<IActionResult> GetMyLeadsAsLister([FromQuery] string? status = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            var result = await _leadService.GetMyLeadsAsListerAsync(userId, status);

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
                    CreatedAt = l.CreatedAt ?? DateTime.UtcNow
                }).ToList();

                return Ok(new { success = true, data = leadViewModels });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpGet("MyLeadsAsSeeker")]
        [Authorize]
        public async Task<IActionResult> GetMyLeadsAsSeeker()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
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
                    CreatedAt = l.CreatedAt ?? DateTime.UtcNow
                }).ToList();

                return Ok(new { success = true, data = leadViewModels });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost("UpdateStatus")]
        [Authorize]
        public async Task<IActionResult> UpdateLeadStatus([FromBody] UpdateLeadStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            var result = await _leadService.UpdateLeadStatusAsync(dto.LeadId, userId, dto.Status, dto.ListerNote);

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpGet("Statistics")]
        [Authorize]
        public async Task<IActionResult> GetStatistics()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            var result = await _leadService.GetLeadStatisticsAsync(userId);

            if (result.Success && result.Data != null)
            {
                var viewModel = new LeadStatisticsViewModel
                {
                    TotalLeads = result.Data.TotalLeads,
                    NewLeads = result.Data.NewLeads,
                    ContactedLeads = result.Data.ContactedLeads,
                    ClosedLeads = result.Data.ClosedLeads,
                    RecentLeads = result.Data.RecentLeads?.Select(l => new LeadViewModel
                    {
                        Id = l.Id,
                        ListingId = l.ListingId,
                        ListingTitle = l.Listing?.Title ?? "N/A",
                        ListingAddress = $"{l.Listing?.StreetName}, {l.Listing?.Ward}",
                        SeekerName = l.Seeker?.DisplayName ?? "N/A",
                        SeekerEmail = l.Seeker?.Email ?? "N/A",
                        Status = l.Status ?? "New",
                        CreatedAt = l.CreatedAt ?? DateTime.UtcNow
                    }).ToList() ?? new List<LeadViewModel>()
                };

                return Ok(new { success = true, data = viewModel });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteLead(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            var result = await _leadService.DeleteLeadAsync(id, userId);

            if (result.Success)
            {
                return Ok(new { success = true, message = result.Message });
            }

            return BadRequest(new { success = false, message = result.Message });
        }
    }
}
