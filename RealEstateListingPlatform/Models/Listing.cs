using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateListingPlatform.Models
{
    public class Listing
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ListerId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PropertyType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(250)]
        public string? StreetName { get; set; }

        [StringLength(100)]
        public string? Area { get; set; }

        [StringLength(100)]
        public string? Ward { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? HouseNumber { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}