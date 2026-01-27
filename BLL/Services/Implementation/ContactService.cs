using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementation
{
    public class ContactService : IContactService
    {
        private readonly IUserRepository _userRepository;
        private readonly IListingRepository _listingRepository;
        private readonly RealEstateListingPlatformContext _context;

        public ContactService(
            IUserRepository userRepository,
            IListingRepository listingRepository,
            RealEstateListingPlatformContext context)
        {
            _userRepository = userRepository;
            _listingRepository = listingRepository;
            _context = context;
        }

        public async Task<ContactResult> ContactListingOwnerAsync(Guid listingId, Guid seekerId, string message, DateTime? appointmentDate)
        {
            try
            {
                // Get listing and owner
                var listing = await _listingRepository.GetByIdAsync(listingId);
                if (listing == null)
                {
                    return new ContactResult { Success = false, Message = "Listing not found." };
                }

                var seeker = await _userRepository.GetUserById(seekerId);
                if (seeker == null)
                {
                    return new ContactResult { Success = false, Message = "Seeker not found." };
                }

                // Create lead
                var lead = new Lead
                {
                    Id = Guid.NewGuid(),
                    ListingId = listingId,
                    SeekerId = seekerId,
                    ListerId = listing.ListerId,
                    Status = "New",
                    Message = message,
                    AppointmentDate = appointmentDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Leads.Add(lead);

                // Create notification for listing owner
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = listing.ListerId,
                    Title = "New Contact Request",
                    Message = $"{seeker.DisplayName} has contacted you about your listing: {listing.Title}",
                    IsRead = false,
                    Type = "Lead",
                    RelatedLink = $"/Contact/LeadDetail/{lead.Id}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                return new ContactResult
                {
                    Success = true,
                    Message = "Contact request sent successfully.",
                    LeadId = lead.Id,
                    NotificationId = notification.Id
                };
            }
            catch (Exception ex)
            {
                return new ContactResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<Lead>> GetUserLeadsAsync(Guid userId)
        {
            return await _context.Leads
                .Include(l => l.Listing)
                    .ThenInclude(listing => listing.ListingMedia)
                .Include(l => l.Listing)
                    .ThenInclude(listing => listing.Lister)
                .Include(l => l.Seeker)
                .Include(l => l.Lister)
                .Where(l => l.SeekerId == userId || l.ListerId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Lead?> GetLeadByIdAsync(Guid leadId)
        {
            return await _context.Leads
                .Include(l => l.Listing)
                    .ThenInclude(listing => listing.ListingMedia)
                .Include(l => l.Listing)
                    .ThenInclude(listing => listing.Lister)
                .Include(l => l.Seeker)
                .Include(l => l.Lister)
                .FirstOrDefaultAsync(l => l.Id == leadId);
        }

        public async Task<bool> UpdateLeadStatusAsync(Guid leadId, string status, string? listerNote)
        {
            try
            {
                var lead = await _context.Leads.FindAsync(leadId);
                if (lead == null) return false;

                lead.Status = status;
                if (!string.IsNullOrEmpty(listerNote))
                {
                    lead.ListerNote = listerNote;
                }

                // Create notification for seeker when status changes
                if (status == "Contacted" || status == "Closed" || status == "Rejected")
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = lead.SeekerId,
                        Title = $"Lead Status Updated",
                        Message = $"Your contact request for listing '{lead.Listing?.Title}' has been {status.ToLower()}.",
                        IsRead = false,
                        Type = "LeadUpdate",
                        RelatedLink = $"/Contact/LeadDetail/{leadId}",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(Guid userId, string title, string message, string type, string? relatedLink)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    Type = type,
                    RelatedLink = relatedLink,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null) return false;

                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && (!n.IsRead.HasValue || !n.IsRead.Value))
                .CountAsync();
        }
    }
}