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
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
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

        // GET: api/payment/history/all - lấy toàn bộ lịch sử giao dịch với phân trang cho người dùng đã xác thực
        [HttpGet("history/all")]
        public async Task<IActionResult> GetAllTransactionHistory([FromQuery] PageRequest request)
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

        // GET: api/payment/payos/return - sửa lý PayOS return URL
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

        // POST: api/payment/payos/webhook - xử lý webhook từ PayOS
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
