using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;

namespace RealEstateListingPlatform.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        private readonly IPackageService _packageService;
        private readonly IPaymentService _paymentService;

        public PackageController(IPackageService packageService, IPaymentService paymentService)
        {
            _packageService = packageService;
            _paymentService = paymentService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        // GET: Package/Index - Browse available packages
        [HttpGet]
        public async Task<IActionResult> Index(string? type = null)
        {
            ServiceResult<List<PackageDto>> result;
            
            if (!string.IsNullOrEmpty(type))
            {
                result = await _packageService.GetPackagesByTypeAsync(type);
                ViewBag.PackageType = type;
            }
            else
            {
                result = await _packageService.GetActivePackagesAsync();
            }

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<PackageDto>());
            }

            return View(result.Data);
        }

        // GET: Package/MyPackages - User's purchased packages
        [HttpGet]
        public async Task<IActionResult> MyPackages()
        {
            var userId = GetCurrentUserId();
            var result = await _packageService.GetUserPackagesAsync(userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<UserPackageDto>());
            }

            var activeResult = await _packageService.GetActiveUserPackagesAsync(userId);
            ViewBag.ActivePackages = activeResult.Data ?? new List<UserPackageDto>();

            return View(result.Data);
        }

        // GET: Package/Purchase/{id} - Purchase confirmation page
        [HttpGet]
        public async Task<IActionResult> Purchase(Guid id)
        {
            var result = await _packageService.GetPackageByIdAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Package not found";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // POST: Package/Purchase - Process package purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(Guid packageId, string paymentMethod, string? notes)
        {
            try
            {
                var userId = GetCurrentUserId();

                var purchaseDto = new PurchasePackageDto
                {
                    PackageId = packageId,
                    PaymentMethod = paymentMethod,
                    Notes = notes
                };

                var result = await _packageService.PurchasePackageAsync(userId, purchaseDto);

                if (!result.Success)
                {
                    TempData["Error"] = $"Purchase failed: {result.Message}";
                    return RedirectToAction(nameof(Index));
                }

                // Ensure transaction data is available
                if (result.Data == null)
                {
                    TempData["Error"] = "Failed to create transaction data";
                    return RedirectToAction(nameof(Index));
                }

                var transactionId = result.Data.TransactionId;
                
                // Create PayOS payment link
                var paymentResult = await _paymentService.InitiatePaymentAsync(transactionId, paymentMethod);
                
                if (!paymentResult.Success)
                {
                    TempData["Error"] = $"Payment initiation failed: {paymentResult.Message ?? "Unknown error"}";
                    return RedirectToAction(nameof(Index));
                }

                // Redirect to Payment Process page to show QR code
                return RedirectToAction("Process", "Payment", new { transactionId = transactionId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Package/ApplyToListing - Apply package to listing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyToListing(Guid userPackageId, Guid listingId)
        {
            var userId = GetCurrentUserId();

            var applyDto = new ApplyPackageDto
            {
                UserPackageId = userPackageId,
                ListingId = listingId
            };

            var result = await _packageService.ApplyPackageToListingAsync(userId, applyDto);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, message = result.Message });
        }

        // POST: Package/Boost/{listingId} - Boost a listing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Boost(Guid listingId, Guid? userPackageId, int boostDays = 7)
        {
            var userId = GetCurrentUserId();

            var boostDto = new BoostListingDto
            {
                ListingId = listingId,
                UserPackageId = userPackageId,
                BoostDays = boostDays
            };

            var result = await _packageService.BoostListingAsync(userId, boostDto);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Details", "Lister", new { id = listingId });
            }

            TempData["Success"] = "Listing boosted successfully!";
            return RedirectToAction("Details", "Lister", new { id = listingId });
        }

        // GET: Package/CanCreateListing - Check if user can create listing (AJAX)
        [HttpGet]
        public async Task<IActionResult> CanCreateListing()
        {
            var userId = GetCurrentUserId();
            var result = await _packageService.CanUserCreateListingAsync(userId);

            return Json(new { 
                success = result.Success, 
                canCreate = result.Success && result.Data, 
                message = result.Message 
            });
        }

        // GET: Package/Details/{id} - Package details
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _packageService.GetPackageByIdAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Package not found";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // GET: Package/CheckPhotoLimit - Check photo limit for listing (AJAX)
        [HttpGet]
        public async Task<IActionResult> CheckPhotoLimit(Guid? listingId)
        {
            var userId = GetCurrentUserId();
            var result = await _packageService.GetAvailablePhotosForListingAsync(userId, listingId);

            return Json(new { 
                success = result.Success, 
                photoLimit = result.Data, 
                message = result.Message 
            });
        }

        // GET: Package/CheckVideoPermission - Check if user can upload videos (AJAX)
        [HttpGet]
        public async Task<IActionResult> CheckVideoPermission(Guid? listingId)
        {
            var userId = GetCurrentUserId();
            
            // If listing exists, check its AllowVideo property
            if (listingId.HasValue)
            {
                // Note: Would need IListingService injected to check listing
                // For now, check user's active packages
            }
            
            // Check active video packages
            var packages = await _packageService.GetActiveUserPackagesAsync(userId);
            if (packages.Success && packages.Data != null)
            {
                var hasVideoPackage = packages.Data.Any(p => 
                    p.Package.PackageType == "VIDEO_UPLOAD" && 
                    p.VideoAvailable && 
                    p.Status == "Active");
                    
                return Json(new { 
                    allowed = hasVideoPackage,
                    message = hasVideoPackage ? "Video upload enabled" : "Purchase video package to enable"
                });
            }
            
            return Json(new { allowed = false, message = "Video upload not available" });
        }

        // GET: Package/GetMyActivePackages - Get user's active packages (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetMyActivePackages()
        {
            var userId = GetCurrentUserId();
            var result = await _packageService.GetActiveUserPackagesAsync(userId);
            
            if (!result.Success)
                return Json(new { success = false, message = result.Message });
            
            var packages = result.Data?.Select(p => new {
                id = p.Id,
                name = p.Package.Name,
                type = p.Package.PackageType,
                photoLimit = p.Package.PhotoLimit,
                allowVideo = p.VideoAvailable,
                remainingListings = p.RemainingListings,
                remainingPhotos = p.RemainingPhotos,
                expiresAt = p.ExpiresAt?.ToString("yyyy-MM-dd")
            });
            
            return Json(new { success = true, data = packages });
        }
    }
}
