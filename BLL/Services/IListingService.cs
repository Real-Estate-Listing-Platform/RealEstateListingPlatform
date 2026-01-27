using BLL.DTOs;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IListingService
    {
        public Task<List<Listing>> GetListings();
        Task<IEnumerable<Listing>> GetPendingListingsAsync();
        Task<IEnumerable<Listing>> GetPublishedListingsAsync();
        Task<IEnumerable<Listing>> GetPublishedByTypeAsync(string type);
        Task<IEnumerable<Listing>> GetByTypeAsync(string type);
        Task<Listing> GetByIdAsync(Guid id);
        Task<bool> ApproveListingAsync(Guid id);
        Task<bool> RejectListingAsync(Guid id);

        // Create
        Task<ServiceResult<Listing>> CreateListingAsync(ListingCreateDto dto, Guid listerId, List<IFormFile>? mediaFiles = null);

        // Read (Enhanced)
        Task<ServiceResult<Listing>> GetListingByIdAsync(Guid id);
        Task<ServiceResult<List<Listing>>> GetMyListingsAsync(Guid listerId);
        Task<ServiceResult<PaginatedResult<Listing>>> GetMyListingsFilteredAsync(Guid listerId, ListingFilterParameters parameters);
        Task<ServiceResult<Listing>> GetListingWithMediaAsync(Guid id);

        // Update
        Task<ServiceResult<Listing>> UpdateListingAsync(Guid id, ListingUpdateDto dto, Guid userId, List<IFormFile>? mediaFiles = null);
        Task<ServiceResult<bool>> SubmitForReviewAsync(Guid id, Guid userId);

        // Delete
        Task<ServiceResult<bool>> DeleteListingAsync(Guid id, Guid userId, bool isAdmin = false);

        // Media Management
        Task<ServiceResult<bool>> AddMediaToListingAsync(Guid listingId, IFormFile file, string mediaType);
        Task<ServiceResult<bool>> DeleteMediaAsync(Guid mediaId, Guid userId);
        Task<ServiceResult<List<ListingMedia>>> GetListingMediaAsync(Guid listingId);

        // Validation
        Task<ServiceResult<bool>> ValidateListingDataAsync(ListingCreateDto dto);
        Task<bool> CanUserModifyListingAsync(Guid listingId, Guid userId);
    }
}
