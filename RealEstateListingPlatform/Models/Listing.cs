using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateListingPlatform.Models
{
    public class Listing
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Lister is required")]
        public Guid ListerId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose transaction type")]
        public string TransactionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose property type")]
        public string PropertyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Street is required")]
        public string StreetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area is required")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ward is required")]
        public string Ward { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "House number is required")]
        public string HouseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude is required")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        public decimal Longitude { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; }
    }
}