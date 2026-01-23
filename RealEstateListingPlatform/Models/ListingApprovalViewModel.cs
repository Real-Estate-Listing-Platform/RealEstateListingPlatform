namespace RealEstateListingPlatform.Models
{
    public class ListingApprovalViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string ListerName { get; set; } = string.Empty;        
        public string Status { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Floors { get; set; }
        public string LegalStatus { get; set; } = string.Empty;
        public string FurnitureStatus { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
