using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using RealEstateListingPlatform.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace RealEstateListingPlatform.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;
        private readonly IListingService _listingService;
        private readonly IUserService _userService;

        public ContactController(IContactService contactService, IListingService listingService, IUserService userService)
        {
            _contactService = contactService;
            _listingService = listingService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult ContactOwner(Guid listingId)
        {
            var model = new ContactViewModel
            {
                ListingId = listingId
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactOwner(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Please login to contact the property owner.";
                return RedirectToAction("Login", "Account");
            }

            var result = await _contactService.ContactListingOwnerAsync(
                model.ListingId,
                userId,
                model.Message,
                model.AppointmentDate);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Your contact request has been sent successfully! The owner will be notified.";
                return RedirectToAction("PropertyDetail", "Listings", new { id = model.ListingId });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyLeads()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            var leads = await _contactService.GetUserLeadsAsync(userId);
            ViewData["CurrentUserId"] = userId;
            return View(leads);
        }

        [HttpGet]
        public async Task<IActionResult> LeadDetail(Guid id)
        {
            var lead = await _contactService.GetLeadByIdAsync(id);
            if (lead == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            if (userId != lead.SeekerId && userId != lead.ListerId)
            {
                return Forbid();
            }

            return View(lead);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLeadStatus(Guid leadId, string status, string? listerNote)
        {
            var success = await _contactService.UpdateLeadStatusAsync(leadId, status, listerNote);
            if (success)
            {
                TempData["SuccessMessage"] = "Lead status updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update lead status.";
            }

            return RedirectToAction("LeadDetail", new { id = leadId });
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = await _contactService.GetUserNotificationsAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var success = await _contactService.MarkNotificationAsReadAsync(notificationId);
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var notifications = await _contactService.GetUserNotificationsAsync(userId);
            var unreadNotifications = notifications.Where(n => !n.IsRead.HasValue || !n.IsRead.Value);

            foreach (var notification in unreadNotifications)
            {
                await _contactService.MarkNotificationAsReadAsync(notification.Id);
            }

            return Json(new { success = true, count = unreadNotifications.Count() });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificationCount()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Json(new { count = 0 });
            }

            var count = await _contactService.GetUnreadNotificationCountAsync(userId);
            return Json(new { count = count });
        }

        private Guid GetCurrentUserId()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Guid.Empty;
            }

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split(':');
                if (parts.Length >= 2 && Guid.TryParse(parts[1], out var userId))
                {
                    return userId;
                }
            }
            catch
            {
                // Token is invalid
            }

            return Guid.Empty;
        }
    }

    public class ContactViewModel
    {
        public Guid ListingId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Preferred Appointment Date")]
        public DateTime? AppointmentDate { get; set; }
    }
}