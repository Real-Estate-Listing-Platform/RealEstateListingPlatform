using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IPriceHistoryService _priceHistoryService;
        private readonly IAuditService _auditService;
        private readonly IPackageService _packageService;
        private readonly IUserRepository _userRepository;

        public ListingService(
            IListingRepository listingRepository,
            IPriceHistoryService priceHistoryService,
            IAuditService auditService,
            IPackageService packageService,
            IUserRepository userRepository)
        { 
            _listingRepository = listingRepository;
            _priceHistoryService = priceHistoryService;
            _auditService = auditService;
            _packageService = packageService;
            _userRepository = userRepository;
        }

        // Existing methods
        public async Task<List<Listing>> GetListings()
        {
            return await _listingRepository.GetListings();
        }

        public async Task<IEnumerable<Listing>> GetPendingListingsAsync()
        {
            var result = await _listingRepository.GetPendingListingsAsync();
            if (result == null)
            {
                return Enumerable.Empty<Listing>();
            }
            return result;
        }

        public async Task<IEnumerable<Listing>> GetByTypeAsync(String type)
        {
            var listings = await _listingRepository.GetPendingListingsAsync();
            if (listings == null)
            {
                return Enumerable.Empty<Listing>();
            }
            var filteredListings = listings.Where(l => l.TransactionType == type);
            return filteredListings;
        }

        public async Task<Listing> GetByIdAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            return listing!;
        }
        
        public async Task<bool> ApproveListingAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null) return false;

            if (listing == null || listing.Status != "PendingReview")
            {
                return false;
            }

            listing.Status = "Published";
            await _listingRepository.UpdateAsync(listing);
            return true;
        }

        public async Task<bool> RejectListingAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null) return false;
            
            if (listing.Status != "PendingReview")
            {
                return false;
            }

            // Refund package slot if listing was using a paid package
            if (!listing.IsFreeListingorder && listing.UserPackageId.HasValue)
            {
                var refundResult = await _packageService.RefundListingSlotAsync(listing.UserPackageId.Value);
                if (refundResult.Success)
                {
                    // Log successful refund
                    await _auditService.LogAsync("PackageRefunded", listing.ListerId, listing.Id, "Listing");
                }
            }

            listing.Status = "Rejected";
            await _listingRepository.UpdateAsync(listing);
            
            // Log rejection audit
            await _auditService.LogAsync("ListingRejected", listing.ListerId, listing.Id, "Listing");
            
            return true;
        }

        // Create
        public async Task<ServiceResult<Listing>> CreateListingAsync(ListingCreateDto dto, Guid listerId, List<IFormFile>? mediaFiles = null)
        {
            // Validate input
            var validationResult = await ValidateListingDataAsync(dto);
            if (!validationResult.Success)
                return ServiceResult<Listing>.FailureResult(validationResult.Message ?? "Validation failed", validationResult.Errors);

            // Check if user can create a listing
            var canCreateResult = await _packageService.CanUserCreateListingAsync(listerId);
            if (!canCreateResult.Success)
                return ServiceResult<Listing>.FailureResult(canCreateResult.Message ?? "Cannot create listing");

            // Get user to check free listing availability
            var user = await _userRepository.GetUserById(listerId);
            if (user == null)
                return ServiceResult<Listing>.FailureResult("User not found");

            // Check for active free listings
            var userListings = await _listingRepository.GetListingsByListerIdAsync(listerId);
            var activeFreeListings = userListings.Count(l => 
                l.IsFreeListingorder && 
                (l.Status == "Published" || l.Status == "PendingReview") &&
                (!l.ExpirationDate.HasValue || l.ExpirationDate > DateTime.UtcNow));

            bool isFreeListingorder = activeFreeListings < user.MaxFreeListings;
            Guid? userPackageId = null;
            int maxPhotos = 5;
            bool allowVideo = false;
            DateTime expirationDate = DateTime.UtcNow.AddDays(30);

            // If not a free listing, find and consume a package
            if (!isFreeListingorder)
            {
                var activePackages = await _packageService.GetActiveUserPackagesAsync(listerId);
                if (!activePackages.Success || activePackages.Data == null)
                    return ServiceResult<Listing>.FailureResult("No available packages found");

                var availablePackage = activePackages.Data
                    .FirstOrDefault(up => 
                        up.Package.PackageType == "ADDITIONAL_LISTING" &&
                        up.Status == "Active" &&
                        up.RemainingListings.HasValue &&
                        up.RemainingListings > 0);

                if (availablePackage == null)
                    return ServiceResult<Listing>.FailureResult("No available listing package found. Please purchase an additional listing package.");

                userPackageId = availablePackage.Id;
                maxPhotos = availablePackage.Package.PhotoLimit ?? 5;
                allowVideo = availablePackage.VideoAvailable;
                if (availablePackage.ExpiresAt.HasValue)
                    expirationDate = availablePackage.ExpiresAt.Value;
            }

            // Map DTO to entity
            var listing = new Listing
            {
                ListerId = listerId,
                Title = dto.Title,
                Description = dto.Description,
                TransactionType = dto.TransactionType,
                PropertyType = dto.PropertyType,
                Price = dto.Price,
                StreetName = dto.StreetName,
                Ward = dto.Ward,
                District = dto.District,
                City = dto.City,
                Area = dto.Area,
                HouseNumber = dto.HouseNumber,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Bedrooms = dto.Bedrooms,
                Bathrooms = dto.Bathrooms,
                Floors = dto.Floors,
                LegalStatus = dto.LegalStatus,
                FurnitureStatus = dto.FurnitureStatus,
                Direction = dto.Direction,
                UserPackageId = userPackageId,
                IsFreeListingorder = isFreeListingorder,
                MaxPhotos = maxPhotos,
                AllowVideo = allowVideo,
                ExpirationDate = expirationDate,
                IsBoosted = false
            };

            // Save to database
            var created = await _listingRepository.CreateAsync(listing);

            // Consume the package if not a free listing
            if (!isFreeListingorder && userPackageId.HasValue)
            {
                var consumeResult = await _packageService.ConsumeListingSlotAsync(userPackageId.Value);
                if (!consumeResult.Success)
                {
                    // Log warning but don't fail the entire operation since listing is already created
                    // In production, you might want to implement compensation logic here
                    await _auditService.LogAsync("PackageConsumptionFailed", listerId, created.Id, "Listing");
                }
            }

            // Handle media uploads
            if (mediaFiles != null && mediaFiles.Any())
            {
                int sortOrder = 0;
                foreach (var file in mediaFiles)
                {
                    var mediaType = file.ContentType.StartsWith("image") ? "image" : "video";
                    var uploadResult = await SaveMediaFileAsync(created.Id, file, mediaType, sortOrder++);
                    
                    if (!uploadResult.Success)
                    {
                        // Log warning but don't fail the entire operation
                        continue;
                    }
                }
            }

            // Log audit trail
            await _auditService.LogAsync("ListingCreated", listerId, listing.Id, "Listing");

            return ServiceResult<Listing>.SuccessResult(created, "Listing created successfully");
        }

        // Read (Enhanced)
        public async Task<ServiceResult<Listing>> GetListingByIdAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return ServiceResult<Listing>.FailureResult("Listing not found");

            return ServiceResult<Listing>.SuccessResult(listing);
        }

        public async Task<ServiceResult<List<Listing>>> GetMyListingsAsync(Guid listerId)
        {
            var listings = await _listingRepository.GetListingsByListerIdAsync(listerId);
            return ServiceResult<List<Listing>>.SuccessResult(listings);
        }

        public async Task<ServiceResult<PaginatedResult<Listing>>> GetMyListingsFilteredAsync(Guid listerId, ListingFilterParameters parameters)
        {
            var (items, totalCount) = await _listingRepository.GetListingsFilteredAsync(
                listerId,
                parameters.SearchTerm,
                parameters.Status,
                parameters.TransactionType,
                parameters.PropertyType,
                parameters.City,
                parameters.District,
                parameters.MinPrice,
                parameters.MaxPrice,
                parameters.SortBy,
                parameters.SortOrder,
                parameters.PageNumber,
                parameters.PageSize);

            var paginatedResult = new PaginatedResult<Listing>
            {
                Items = items,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };

            return ServiceResult<PaginatedResult<Listing>>.SuccessResult(paginatedResult);
        }

        public async Task<ServiceResult<Listing>> GetListingWithMediaAsync(Guid id)
        {
            var listing = await _listingRepository.GetListingWithMediaAsync(id);
            if (listing == null)
                return ServiceResult<Listing>.FailureResult("Listing not found");

            return ServiceResult<Listing>.SuccessResult(listing);
        }

        // Update
        public async Task<ServiceResult<Listing>> UpdateListingAsync(Guid id, ListingUpdateDto dto, Guid userId, List<IFormFile>? mediaFiles = null)
        {
            // Check if listing exists
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return ServiceResult<Listing>.FailureResult("Listing not found");

            // Verify ownership
            if (!await CanUserModifyListingAsync(id, userId))
                return ServiceResult<Listing>.FailureResult("You are not authorized to modify this listing");

            // Track price changes
            if (dto.Price.HasValue && dto.Price.Value != listing.Price)
            {
                await _priceHistoryService.RecordPriceChangeAsync(id, listing.Price, dto.Price.Value, userId);
            }

            // Update fields (only update non-null values from DTO)
            if (!string.IsNullOrEmpty(dto.Title)) listing.Title = dto.Title;
            if (dto.Description != null) listing.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.TransactionType)) listing.TransactionType = dto.TransactionType;
            if (!string.IsNullOrEmpty(dto.PropertyType)) listing.PropertyType = dto.PropertyType;
            if (dto.Price.HasValue) listing.Price = dto.Price.Value;
            if (dto.StreetName != null) listing.StreetName = dto.StreetName;
            if (dto.Ward != null) listing.Ward = dto.Ward;
            if (dto.District != null) listing.District = dto.District;
            if (dto.City != null) listing.City = dto.City;
            if (dto.Area != null) listing.Area = dto.Area;
            if (dto.HouseNumber != null) listing.HouseNumber = dto.HouseNumber;
            if (dto.Latitude.HasValue) listing.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) listing.Longitude = dto.Longitude;
            if (dto.Bedrooms.HasValue) listing.Bedrooms = dto.Bedrooms;
            if (dto.Bathrooms.HasValue) listing.Bathrooms = dto.Bathrooms;
            if (dto.Floors.HasValue) listing.Floors = dto.Floors;
            if (dto.LegalStatus != null) listing.LegalStatus = dto.LegalStatus;
            if (dto.FurnitureStatus != null) listing.FurnitureStatus = dto.FurnitureStatus;
            if (dto.Direction != null) listing.Direction = dto.Direction;

            await _listingRepository.UpdateAsync(listing);

            // Handle media uploads
            if (mediaFiles != null && mediaFiles.Any())
            {
                var existingMedia = await _listingRepository.GetMediaByListingIdAsync(id);
                int sortOrder = existingMedia.Count;
                
                foreach (var file in mediaFiles)
                {
                    var mediaType = file.ContentType.StartsWith("image") ? "image" : "video";
                    await SaveMediaFileAsync(id, file, mediaType, sortOrder++);
                }
            }

            // Log audit trail
            await _auditService.LogAsync("ListingUpdated", userId, listing.Id, "Listing");

            return ServiceResult<Listing>.SuccessResult(listing, "Listing updated successfully");
        }

        public async Task<ServiceResult<bool>> SubmitForReviewAsync(Guid id, Guid userId)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return ServiceResult<bool>.FailureResult("Listing not found");

            if (!await CanUserModifyListingAsync(id, userId))
                return ServiceResult<bool>.FailureResult("You are not authorized to modify this listing");

            // Allow submission from Draft or from newly created listings
            if (listing.Status != "Draft" && listing.Status != "PendingReview")
                return ServiceResult<bool>.FailureResult("Only draft listings can be submitted for review");

            // Validate required fields
            if (string.IsNullOrEmpty(listing.Title) || listing.Price <= 0)
                return ServiceResult<bool>.FailureResult("Please complete all required fields before submitting");

            listing.Status = "PendingReview";
            await _listingRepository.UpdateAsync(listing);

            await _auditService.LogAsync("ListingSubmittedForReview", userId, id, "Listing");

            return ServiceResult<bool>.SuccessResult(true, "Listing submitted for review");
        }

        // Delete
        public async Task<ServiceResult<bool>> DeleteListingAsync(Guid id, Guid userId, bool isAdmin = false)
        {
            var listing = await _listingRepository.GetListingWithMediaAsync(id);
            if (listing == null)
                return ServiceResult<bool>.FailureResult("Listing not found");

            // Verify ownership or admin role
            if (!isAdmin && !await CanUserModifyListingAsync(id, userId))
                return ServiceResult<bool>.FailureResult("You are not authorized to delete this listing");

            // Delete physical media files from storage
            if (listing.ListingMedia != null && listing.ListingMedia.Any())
            {
                foreach (var media in listing.ListingMedia)
                {
                    DeletePhysicalFile(media.Url);
                }
            }

            // Log audit trail before deletion
            await _auditService.LogAsync("ListingDeleted", userId, id, "Listing");

            // Hard delete
            var deleted = await _listingRepository.DeleteAsync(id);
            if (!deleted)
                return ServiceResult<bool>.FailureResult("Failed to delete listing");

            return ServiceResult<bool>.SuccessResult(true, "Listing deleted successfully");
        }

        // Media Management
        public async Task<ServiceResult<bool>> AddMediaToListingAsync(Guid listingId, IFormFile file, string mediaType)
        {
            // Verify listing exists
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
                return ServiceResult<bool>.FailureResult("Listing not found");

            // Check video permission
            if (mediaType.ToLower() == "video" && !listing.AllowVideo)
                return ServiceResult<bool>.FailureResult("Video upload not allowed. Please purchase video upload package.");

            // Check photo limit
            var existingMedia = await _listingRepository.GetMediaByListingIdAsync(listingId);
            if (mediaType.ToLower() == "image")
            {
                var photoCount = existingMedia.Count(m => m.MediaType == "image");
                if (photoCount >= listing.MaxPhotos)
                    return ServiceResult<bool>.FailureResult($"Photo limit reached ({listing.MaxPhotos} photos). Purchase photo pack to add more.");
            }

            // Validate file
            if (!IsValidMediaFile(file, mediaType))
                return ServiceResult<bool>.FailureResult("Invalid file type or size. Max 10MB for images/videos.");

            // Get current media count for sort order
            int sortOrder = existingMedia.Count;

            // Save file
            var result = await SaveMediaFileAsync(listingId, file, mediaType, sortOrder);
            return result;
        }

        public async Task<ServiceResult<bool>> DeleteMediaAsync(Guid mediaId, Guid userId)
        {
            var media = await _listingRepository.GetMediaByListingIdAsync(Guid.Empty);
            var targetMedia = media.FirstOrDefault(m => m.Id == mediaId);
            
            if (targetMedia == null)
                return ServiceResult<bool>.FailureResult("Media not found");

            // Verify ownership
            if (!await CanUserModifyListingAsync(targetMedia.ListingId, userId))
                return ServiceResult<bool>.FailureResult("You are not authorized to delete this media");

            // Delete physical file
            DeletePhysicalFile(targetMedia.Url);

            // Delete from database
            await _listingRepository.DeleteMediaAsync(mediaId);

            return ServiceResult<bool>.SuccessResult(true, "Media deleted successfully");
        }

        public async Task<ServiceResult<List<ListingMedia>>> GetListingMediaAsync(Guid listingId)
        {
            var media = await _listingRepository.GetMediaByListingIdAsync(listingId);
            return ServiceResult<List<ListingMedia>>.SuccessResult(media);
        }

        // Validation
        public async Task<ServiceResult<bool>> ValidateListingDataAsync(ListingCreateDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add("Title is required");

            if (string.IsNullOrWhiteSpace(dto.TransactionType))
                errors.Add("Transaction type is required");

            if (string.IsNullOrWhiteSpace(dto.PropertyType))
                errors.Add("Property type is required");

            if (dto.Price <= 0)
                errors.Add("Price must be greater than 0");

            if (errors.Any())
                return ServiceResult<bool>.FailureResult("Validation failed", errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        public async Task<bool> CanUserModifyListingAsync(Guid listingId, Guid userId)
        {
            return await _listingRepository.IsOwnerAsync(listingId, userId);
        }

        // Helper Methods
        private bool IsValidMediaFile(IFormFile file, string mediaType)
        {
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize) return false;

            var allowedExtensions = mediaType.ToLower() == "image"
                ? new[] { ".jpg", ".jpeg", ".png", ".webp" }
                : new[] { ".mp4", ".avi", ".mov" };

            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        private async Task<ServiceResult<bool>> SaveMediaFileAsync(Guid listingId, IFormFile file, string mediaType, int sortOrder)
        {
            try
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", "listings");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var media = new ListingMedia
                {
                    ListingId = listingId,
                    MediaType = mediaType,
                    Url = $"/uploads/listings/{fileName}",
                    SortOrder = sortOrder
                };

                await _listingRepository.AddMediaAsync(listingId, media);

                return ServiceResult<bool>.SuccessResult(true, "Media uploaded successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to upload media: {ex.Message}");
            }
        }

        private void DeletePhysicalFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var filePath = Path.Combine("wwwroot", url.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Log error but don't throw
            }
        }
    }
}
