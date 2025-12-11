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

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/payments")]
    [Authorize(Roles = "Student")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly ILogger<PaymentController> _logger;
        private readonly RequestPaymentValidator _requestValidator;
        private readonly CompletePaymentValidator _completeValidator;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            ILogger<PaymentController> logger,
            RequestPaymentValidator requestValidator,
            CompletePaymentValidator completeValidator,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _logger = logger;
            _requestValidator = requestValidator;
            _completeValidator = completeValidator;
            _configuration = configuration;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user credentials");
            }
            return userId;
        }

        // POST: api/payment/process - Create payment request and generate payment URL
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] requestPayment request)
        {
            var validationResult = await _requestValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userId = GetCurrentUserId();
            var result = await _paymentService.ProcessPaymentAsync(userId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/payment/confirm - Confirm and complete payment transaction
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] CompletePayment paymentDto)
        {
            var validationResult = await _completeValidator.ValidateAsync(paymentDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userId = GetCurrentUserId();
            var result = await _paymentService.ConfirmPaymentAsync(paymentDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/payment/history - Get paginated transaction history for authenticated user
        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
            {
                return BadRequest(new { message = "Page number must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Page size must be between 1 and 100" });
            }

            var userId = GetCurrentUserId();
            var result = await _paymentService.GetTransactionHistoryAsync(userId, pageNumber, pageSize);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/payment/transaction/{paymentId} - Get detailed information about a specific transaction
        [HttpGet("transaction/{paymentId}")]
        public async Task<IActionResult> GetTransactionDetail(int paymentId)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.GetTransactionDetailAsync(paymentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/payment/payos/create-link/{paymentId} - Create PayOS payment link
        [HttpPost("payos/create-link/{paymentId}")]
        public async Task<IActionResult> CreatePayOSLink(int paymentId)
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.CreatePayOSPaymentLinkAsync(paymentId, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: api/payment/payos/return - Handle PayOS redirect after payment 
        [HttpGet("payos/return")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] string? code,
            [FromQuery] string? desc,
            [FromQuery] string? data)
        {
            try
            {
                _logger.LogInformation("PayOS return: code={Code}", code);

                var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";

                if (code != "00")
                {
                    _logger.LogWarning("PayOS payment failed: {Desc}", desc);
                    return Redirect($"{frontendUrl}/payment-failed?reason={Uri.EscapeDataString(desc ?? "Payment failed")}");
                }

                if (string.IsNullOrEmpty(data))
                {
                    _logger.LogWarning("PayOS return data is empty");
                    return Redirect($"{frontendUrl}/payment-failed?reason=Invalid data");
                }

                try
                {
                    var webhookData = JsonSerializer.Deserialize<JsonElement>(data);
                    var orderCode = webhookData.GetProperty("orderCode").GetInt64().ToString();

                    var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(orderCode);
                    if (payment == null)
                    {
                        _logger.LogWarning("Payment not found for orderCode {OrderCode}", orderCode);
                        return Redirect($"{frontendUrl}/payment-failed?reason=Payment not found");
                    }

                    _logger.LogInformation("PayOS return successful for Payment {PaymentId}, OrderCode {OrderCode}",
                        payment.PaymentId, orderCode);

                    // Tự động confirm payment nếu chưa được confirm
                    if (payment.Status == PaymentStatus.Pending)
                    {
                        _logger.LogInformation("Auto-confirming Payment {PaymentId} via Return URL", payment.PaymentId);

                        var confirmDto = new CompletePayment
                        {
                            PaymentId = payment.PaymentId,
                            ProductId = payment.ProductId,
                            ProductType = payment.ProductType,
                            Amount = payment.Amount,
                            PaymentMethod = PaymentGateway.PayOs.ToString()
                        };

                        var confirmResult = await _paymentService.ConfirmPaymentAsync(confirmDto, payment.UserId);

                        if (confirmResult.Success)
                        {
                            _logger.LogInformation("Payment {PaymentId} auto-confirmed successfully via Return URL", payment.PaymentId);
                        }
                        else
                        {
                            // Nếu confirm fail (có thể đã được confirm bởi webhook), vẫn redirect success
                           
                            _logger.LogWarning("Payment {PaymentId} confirmation failed or already processed: {Message}",
                                payment.PaymentId, confirmResult.Message);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Payment {PaymentId} already processed with status {Status}",
                            payment.PaymentId, payment.Status);
                    }

                    return Redirect($"{frontendUrl}/payment-success?paymentId={payment.PaymentId}&orderCode={orderCode}");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing PayOS return data");
                    return Redirect($"{frontendUrl}/payment-failed?reason=Invalid data format");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS return");
                var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
                return Redirect($"{frontendUrl}/payment-failed?reason=Server error");
            }
        }

        // POST: api/payment/payos/webhook - Handle PayOS webhook notification
        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookDto webhookData)
        {
            try
            {
                _logger.LogInformation("PayOS webhook received: code={Code}, orderCode={OrderCode}",
                    webhookData.Code, webhookData.OrderCode);

                var isValid = await _payOSService.VerifyWebhookSignature(webhookData.Data, webhookData.Signature);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid PayOS webhook signature");
                    return BadRequest(new { message = "Invalid signature" });
                }

                if (webhookData.Code != "00")
                {
                    _logger.LogWarning("PayOS payment failed: {Desc}", webhookData.Desc);
                    return Ok(new { message = "Payment failed" });
                }

                var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(webhookData.OrderCode.ToString());
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for orderCode {OrderCode}", webhookData.OrderCode);
                    return NotFound(new { message = "Payment not found" });
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    _logger.LogInformation("Payment {PaymentId} already completed", payment.PaymentId);
                    return Ok(new { message = "Already processed", paymentId = payment.PaymentId });
                }

                var confirmDto = new CompletePayment
                {
                    PaymentId = payment.PaymentId,
                    ProductId = payment.ProductId,
                    ProductType = payment.ProductType,
                    Amount = payment.Amount,
                    PaymentMethod = PaymentGateway.PayOs.ToString()
                };

                var result = await _paymentService.ConfirmPaymentAsync(confirmDto, payment.UserId);
                
                if (result.Success)
                {
                    _logger.LogInformation("Payment {PaymentId} confirmed via webhook", payment.PaymentId);
                    return Ok(new { message = "Success", paymentId = payment.PaymentId });
                }
                else
                {
                    _logger.LogError("Failed to confirm payment {PaymentId}: {Message}",
                        payment.PaymentId, result.Message);
                    return StatusCode(500, new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST: api/payment/payos/confirm/{paymentId} - Confirm PayOS payment from frontend
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
