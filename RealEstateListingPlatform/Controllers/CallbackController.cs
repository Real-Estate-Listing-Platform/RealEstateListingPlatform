using BLL.DTOs;
using BLL.Services;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace RealEstateListingPlatform.Controllers
{
    [Route("api")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPackageService _packageService;
        private readonly IPayOSService _payOSService;
        private readonly ITransactionRepository _transactionRepository;

        public CallbackController(
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

        [HttpPost("payment")]
        public async Task<IActionResult> Callback(Webhook webhook)
        {
            Console.WriteLine($"[PayOS Webhook] Received webhook for OrderCode: {webhook.Data.OrderCode}");

            var verifiedData = await _payOSService.VerifyWebhookDataAsync(webhook);

            // Find transaction by order code
            var transaction = await _transactionRepository.GetTransactionByPayOSOrderCodeAsync(verifiedData.OrderCode);

            // Process based on status
            if (verifiedData.Code == "00")
            {

                var completeDto = new CompleteTransactionDto
                {
                    TransactionId = transaction.Id,
                    PaymentReference = verifiedData.Reference ?? verifiedData.OrderCode.ToString(),
                    Notes = $"PayOS webhook - Payment successful"
                };

                transaction.PayOSTransactionId = verifiedData.Reference;
                await _transactionRepository.UpdateTransactionAsync(transaction);

                var completeResult = await _paymentService.CompleteTransactionAsync(completeDto);

                if (completeResult.Success)
                {
                    await _packageService.ActivateUserPackageAsync(transaction.Id);
                }
            }
            else
            {
                await _paymentService.FailTransactionAsync(
                    transaction.Id,
                    $"PayOS payment failed/cancelled with code: {verifiedData.Code}"
                );
            }
            return Ok("ok");
        }
    }
}
