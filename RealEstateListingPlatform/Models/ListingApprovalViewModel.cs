namespace RealEstateListingPlatform.Models
{
    public class ListingApprovalViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string PropertyType { get; set; }
        public decimal Price { get; set; }
        public string Address { get; set; }
        public string ListerName { get; set; }
        public string Description { get; set; } 
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
