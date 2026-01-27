using DAL.Models;

namespace BLL.Services
{
    public interface IContactService
    {
        Task<ContactResult> ContactListingOwnerAsync(Guid listingId, Guid seekerId, string message, DateTime? appointmentDate);
        Task<IEnumerable<Lead>> GetUserLeadsAsync(Guid userId);
        Task<Lead?> GetLeadByIdAsync(Guid leadId);
        Task<bool> UpdateLeadStatusAsync(Guid leadId, string status, string? listerNote);
        Task<bool> SendNotificationAsync(Guid userId, string title, string message, string type, string? relatedLink);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
        Task<int> GetUnreadNotificationCountAsync(Guid userId);
    }

    public class ContactResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? LeadId { get; set; }
        public Guid? NotificationId { get; set; }
    }
}