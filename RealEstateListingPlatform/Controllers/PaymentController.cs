using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using BLL.DTOs;
using System.Security.Claims;
using DAL.Repositories;
using PayOS.Models.Webhooks;

namespace RealEstateListingPlatform.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IPackageService _packageService;
        private readonly IPayOSService _payOSService;
        private readonly ITransactionRepository _transactionRepository;

        public PaymentController(
            IPaymentService paymentService, 
            IPackageService packageService,
            IPayOSService payOSService,
            ITransactionRepository transactionRepository)
        {
            _paymentService = paymentService;
            _packageService = packageService;
            _payOSService = payOSService;
            _transactionRepository = transactionRepository;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        }

        // GET: Payment/Process - Payment gateway redirect
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Process(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Package");
            }

            // Get payment link info
            var dbTransaction = await _transactionRepository.GetTransactionWithDetailsAsync(transactionId);
            if (dbTransaction != null && dbTransaction.PayOSCheckoutUrl != null)
            {
                ViewBag.CheckoutUrl = dbTransaction.PayOSCheckoutUrl;
                ViewBag.OrderCode = dbTransaction.PayOSOrderCode;
                
                // Debug logging
                Console.WriteLine($"[Payment/Process] Transaction ID: {transactionId}");
                Console.WriteLine($"[Payment/Process] Checkout URL: {dbTransaction.PayOSCheckoutUrl}");
                Console.WriteLine($"[Payment/Process] Order Code: {dbTransaction.PayOSOrderCode}");
            }
            else
            {
                Console.WriteLine($"[Payment/Process] WARNING: No PayOS data found for transaction {transactionId}");
            }

            ViewBag.Transaction = transaction.Data;
            return View(transaction.Data);
        }

        // GET: Payment/CheckStatus - Check payment status via AJAX
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckStatus(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                return Json(new { success = false, message = "Transaction not found" });
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            // Check current status from database
            var dbTransaction = await _transactionRepository.GetTransactionWithDetailsAsync(transactionId);
            if (dbTransaction == null)
            {
                return Json(new { success = false, message = "Transaction not found" });
            }

            // If already completed, return success
            if (dbTransaction.Status == "Completed")
            {
                return Json(new { 
                    success = true, 
                    status = "Completed",
                    message = "Payment completed successfully!",
                    redirectUrl = Url.Action("Success", new { transactionId = transactionId })
                });
            }

            // Check with PayOS if payment was made
            if (dbTransaction.PayOSOrderCode.HasValue)
            {
                var paymentInfo = await _payOSService.GetPaymentInfoAsync(dbTransaction.PayOSOrderCode.Value);
                
                if (paymentInfo != null && paymentInfo.Status == "PAID")
                {
                    // Update transaction status
                    var completeDto = new CompleteTransactionDto
                    {
                        TransactionId = transactionId,
                        PaymentReference = paymentInfo.TransactionReference ?? paymentInfo.OrderCode.ToString(),
                        Notes = "Payment verified via status check"
                    };

                    await _paymentService.CompleteTransactionAsync(completeDto);
                    await _packageService.ActivateUserPackageAsync(transactionId);

                    return Json(new { 
                        success = true, 
                        status = "Completed",
                        message = "Payment completed successfully!",
                        redirectUrl = Url.Action("Success", new { transactionId = transactionId })
                    });
                }
                else if (paymentInfo != null && paymentInfo.Status == "CANCELLED")
                {
                    await _paymentService.FailTransactionAsync(transactionId, "Payment cancelled");
                    
                    return Json(new { 
                        success = false, 
                        status = "Cancelled",
                        message = "Payment was cancelled",
                        redirectUrl = Url.Action("Failed", new { transactionId = transactionId })
                    });
                }
            }

            // Still pending
            return Json(new { 
                success = true, 
                status = "Pending",
                message = "Payment is still pending. Please complete the payment."
            });
        }

        // GET: Payment/Callback/Test - Test endpoint to verify webhook is accessible
        [HttpGet("/Payment/Callback")]
        [AllowAnonymous]
        public IActionResult TestWebhook()
        {
            return Ok(new { 
                message = "Webhook endpoint is accessible", 
                timestamp = DateTime.UtcNow,
                method = "GET",
                note = "POST requests are used for actual webhook callbacks"
            });
        }

        // POST: Payment/Callback - Payment gateway callback (webhook)
        

        // GET: Payment/PayOSReturn - PayOS return URL
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] string code,
            [FromQuery] string id,
            [FromQuery] string? cancel,
            [FromQuery] string? status,
            [FromQuery] long orderCode)
        {
            try
            {
                var transaction = await _transactionRepository.GetTransactionByPayOSOrderCodeAsync(orderCode);
                
                if (transaction == null)
                {
                    TempData["Error"] = "Transaction not found";
                    return RedirectToAction("Index", "Package");
                }

                if (code == "00" && status != "CANCELLED")
                {
                    var paymentInfo = await _payOSService.GetPaymentInfoAsync(orderCode);
                    
                    if (paymentInfo != null && paymentInfo.Status == "PAID")
                    {
                        return RedirectToAction(nameof(Success), new { transactionId = transaction.Id });
                    }
                }

                return RedirectToAction(nameof(Failed), new { transactionId = transaction.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing return: {ex.Message}";
                return RedirectToAction("Index", "Package");
            }
        }

        // GET: Payment/PayOSCancel - PayOS cancel URL
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSCancel([FromQuery] long orderCode)
        {
            try
            {
                var transaction = await _transactionRepository.GetTransactionByPayOSOrderCodeAsync(orderCode);
                
                if (transaction != null)
                {
                    await _paymentService.FailTransactionAsync(transaction.Id, "Payment cancelled by user");
                    return RedirectToAction(nameof(Failed), new { transactionId = transaction.Id });
                }

                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing cancellation: {ex.Message}";
                return RedirectToAction("Index", "Package");
            }
        }

        // GET: Payment/Success - Payment success page
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Success(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Package");
            }

            ViewBag.Transaction = transaction.Data;
            return View();
        }

        // GET: Payment/Failed - Payment failed page
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Failed(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction("Index", "Package");
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction("Index", "Package");
            }

            ViewBag.Transaction = transaction.Data;
            return View();
        }

        // POST: Payment/ManualComplete - Manually complete payment (for bank transfer, admin approval)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualComplete(Guid transactionId, string paymentReference, string? notes)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                return Json(new { success = false, message = "Transaction not found" });
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            var completeDto = new CompleteTransactionDto
            {
                TransactionId = transactionId,
                PaymentReference = paymentReference,
                Notes = notes
            };

            var result = await _paymentService.CompleteTransactionAsync(completeDto);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, message = "Payment marked as completed. Awaiting admin approval." });
        }

        // GET: Payment/MyTransactions - User's transaction history
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyTransactions()
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.GetUserTransactionsAsync(userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<TransactionDto>());
            }

            return View(result.Data);
        }

        // POST: Payment/RetryPayment - Retry failed payment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryPayment(Guid transactionId)
        {
            var userId = GetCurrentUserId();
            var transaction = await _paymentService.GetTransactionByIdAsync(transactionId);

            if (!transaction.Success || transaction.Data == null)
            {
                TempData["Error"] = "Transaction not found";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Verify user owns this transaction
            if (transaction.Data.UserId != userId)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Only allow retry for Pending or Failed transactions
            if (transaction.Data.Status != "Pending" && transaction.Data.Status != "Failed")
            {
                TempData["Error"] = "This transaction cannot be retried";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Create new PayOS payment link
            var paymentResult = await _paymentService.InitiatePaymentAsync(
                transactionId, 
                transaction.Data.PaymentMethod ?? "PAYOS"
            );

            if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.Data))
            {
                TempData["Error"] = paymentResult.Message ?? "Failed to create payment link";
                return RedirectToAction(nameof(MyTransactions));
            }

            // Redirect to PayOS payment page
            return Redirect(paymentResult.Data);
        }
    }
}
