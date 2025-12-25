using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LearningEnglish.Application.Validators.Payment;
using System.Text.Json;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Configuration;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/payments")]
    [Authorize(Roles = "Student")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentWebhookQueueRepository _webhookQueueRepository;
        private readonly IPayOSService _payOSService;
        private readonly ILogger<PaymentController> _logger;
        private readonly RequestPaymentValidator _requestValidator;
        private readonly CompletePaymentValidator _completeValidator;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            IPaymentRepository paymentRepository,
            IPaymentWebhookQueueRepository webhookQueueRepository,
            IPayOSService payOSService,
            ILogger<PaymentController> logger,
            RequestPaymentValidator requestValidator,
            CompletePaymentValidator completeValidator,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _paymentRepository = paymentRepository;
            _webhookQueueRepository = webhookQueueRepository;
            _payOSService = payOSService;
            _logger = logger;
            _requestValidator = requestValidator;
            _completeValidator = completeValidator;
            _configuration = configuration;
        }

        // POST: api/payment/process - tạo yêu cầu thanh toán
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] requestPayment request)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.ProcessPaymentAsync(userId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/payment/confirm - xác nhận thanh toán
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] CompletePayment paymentDto)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.ConfirmPaymentAsync(paymentDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/payment/history - lấy lịch sử giao dịch với phân trang cho người dùng đã xác thực
        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] PageRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.GetTransactionHistoryAsync(userId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/payment/transaction/{paymentId} - lây chi tiết giao dịch theo paymentId
        [HttpGet("transaction/{paymentId}")]
        public async Task<IActionResult> GetTransactionDetail(int paymentId)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.GetTransactionDetailAsync(paymentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/payment/payos/create-link/{paymentId} - tạo link thanh toán PayOS
        [HttpPost("payos/create-link/{paymentId}")]
        public async Task<IActionResult> CreatePayOSLink(int paymentId)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.CreatePayOSPaymentLinkAsync(paymentId, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: api/payment/payos/return - xử lý PayOS return URL
        [HttpGet("payos/return")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] string? code,
            [FromQuery] string? desc,
            [FromQuery] string? data)
        {
            var result = await _paymentService.ProcessPayOSReturnAsync(code ?? "", desc ?? "", data ?? "");

            if (result.Success && result.Data != null)
            {
                return Redirect(result.Data.RedirectUrl ?? "http://localhost:3000/payment-failed");
            }
            else
            {
                return Redirect("http://localhost:3000/payment-failed?reason=Server error");
            }
        }

        // POST: api/payment/payos/webhook - xử lý webhook từ PayOS
        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookDto webhookData)
        {
            try
            {
                _logger.LogInformation("📨 PayOS webhook received: code={Code}, orderCode={OrderCode}",
                    webhookData.Code, webhookData.OrderCode);

                // Verify signature
                var isValid = await _payOSService.VerifyWebhookSignature(webhookData.Data, webhookData.Signature);
                if (!isValid)
                {
                    _logger.LogWarning("❌ Invalid PayOS webhook signature");
                    return BadRequest(new { message = "Invalid signature" });
                }

                // Find payment
                var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(webhookData.OrderCode.ToString());
                if (payment == null)
                {
                    _logger.LogWarning("⚠️ Payment not found for orderCode {OrderCode}", webhookData.OrderCode);
                    return NotFound(new { message = "Payment not found" });
                }

                // Save webhook to queue for retry mechanism
                var webhookQueue = new LearningEnglish.Domain.Entities.PaymentWebhookQueue
                {
                    PaymentId = payment.PaymentId,
                    OrderCode = webhookData.OrderCode,
                    WebhookData = JsonSerializer.Serialize(webhookData),
                    Signature = webhookData.Signature,
                    Status = LearningEnglish.Domain.Enums.WebhookStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    RetryCount = 0,
                    MaxRetries = 5
                };

                await _webhookQueueRepository.AddWebhookAsync(webhookQueue);
                await _webhookQueueRepository.SaveChangesAsync();

                _logger.LogInformation("💾 Webhook saved to queue: WebhookId={WebhookId}, PaymentId={PaymentId}",
                    webhookQueue.WebhookId, payment.PaymentId);

                // Try to process immediately
                try
                {
                    // Check if already processed
                    if (payment.Status == PaymentStatus.Completed)
                    {
                        _logger.LogInformation("✅ Payment {PaymentId} already completed", payment.PaymentId);
                        
                        webhookQueue.Status = LearningEnglish.Domain.Enums.WebhookStatus.Processed;
                        webhookQueue.ProcessedAt = DateTime.UtcNow;
                        await _webhookQueueRepository.UpdateWebhookStatusAsync(webhookQueue);
                        await _webhookQueueRepository.SaveChangesAsync();
                        
                        return Ok(new { message = "Already processed", paymentId = payment.PaymentId });
                    }

                    if (webhookData.Code != "00")
                    {
                        _logger.LogWarning("⚠️ PayOS payment failed: {Desc}", webhookData.Desc);
                        
                        webhookQueue.Status = LearningEnglish.Domain.Enums.WebhookStatus.Failed;
                        webhookQueue.LastError = $"Payment failed: {webhookData.Desc}";
                        await _webhookQueueRepository.UpdateWebhookStatusAsync(webhookQueue);
                        await _webhookQueueRepository.SaveChangesAsync();
                        
                        return Ok(new { message = "Payment failed" });
                    }

                    // Process webhook
                    var result = await _paymentService.ProcessWebhookFromQueueAsync(webhookData);

                    if (result.Success)
                    {
                        webhookQueue.Status = LearningEnglish.Domain.Enums.WebhookStatus.Processed;
                        webhookQueue.ProcessedAt = DateTime.UtcNow;
                        await _webhookQueueRepository.UpdateWebhookStatusAsync(webhookQueue);
                        await _webhookQueueRepository.SaveChangesAsync();

                        _logger.LogInformation("✅ Payment {PaymentId} confirmed via webhook", payment.PaymentId);
                        return Ok(new { message = "Success", paymentId = payment.PaymentId });
                    }
                    else
                    {
                        throw new Exception(result.Message ?? "Processing failed");
                    }
                }
                catch (Exception processEx)
                {
                    _logger.LogError(processEx, "❌ Failed to process webhook immediately, will retry later");

                    // Mark for retry
                    webhookQueue.Status = LearningEnglish.Domain.Enums.WebhookStatus.Failed;
                    webhookQueue.RetryCount = 0;
                    webhookQueue.NextRetryAt = DateTime.UtcNow.AddMinutes(1); // Retry trong 1 phút
                    webhookQueue.LastError = processEx.Message;
                    webhookQueue.ErrorStackTrace = processEx.StackTrace;
                    await _webhookQueueRepository.UpdateWebhookStatusAsync(webhookQueue);
                    await _webhookQueueRepository.SaveChangesAsync();

                    _logger.LogInformation("⏰ Webhook scheduled for retry at {NextRetryAt}", webhookQueue.NextRetryAt);

                    // Return success to PayOS để không bị spam retry từ phía PayOS
                    return Ok(new { message = "Webhook queued for processing", paymentId = payment.PaymentId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Critical error processing PayOS webhook");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST: api/payment/payos/confirm/{paymentId} - xác nhận thanh toán PayOS
        [HttpPost("payos/confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayOSPayment(int paymentId)
        {
            var userId = GetCurrentUserId();

            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
            if (payment == null || payment.UserId != userId)
            {
                return NotFound(new { message = "Payment not found" });
            }

            if (!string.IsNullOrEmpty(payment.ProviderTransactionId) &&
                long.TryParse(payment.ProviderTransactionId, out var orderCode))
            {
                var payosInfo = await _payOSService.GetPaymentInformationAsync(orderCode);
                if (!payosInfo.Success || payosInfo.Data == null || payosInfo.Data.Code != "00")
                {
                    return BadRequest(new { message = "Payment not completed on PayOS" });
                }
            }

            var confirmDto = new CompletePayment
            {
                PaymentId = paymentId,
                ProductId = payment.ProductId,
                ProductType = payment.ProductType,
                Amount = payment.Amount,
                PaymentMethod = PaymentGateway.PayOs.ToString()
            };

            var result = await _paymentService.ConfirmPaymentAsync(confirmDto, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
