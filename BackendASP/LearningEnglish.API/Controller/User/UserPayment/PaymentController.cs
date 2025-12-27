using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using Microsoft.Extensions.Configuration;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/payments")]
    [Authorize(Roles = "Student,Teacher")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

       
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] requestPayment request)
        {
            var userId = User.GetUserId();
            var result = await _paymentService.ProcessPaymentAsync(userId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student xác nhận thanh toán
        // FluentValidation: CompletePaymentValidator sẽ tự động validate
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] CompletePayment paymentDto)
        {
            var userId = User.GetUserId();
            var result = await _paymentService.ConfirmPaymentAsync(paymentDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student lấy lịch sử giao dịch (phân trang)
        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] PageRequest request)
        {
            var userId = User.GetUserId();
            var result = await _paymentService.GetTransactionHistoryAsync(userId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student lấy chi tiết giao dịch theo paymentId
        [HttpGet("transaction/{paymentId}")]
        public async Task<IActionResult> GetTransactionDetail(int paymentId)
        {
            var userId = User.GetUserId();
            var result = await _paymentService.GetTransactionDetailAsync(paymentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student tạo link thanh toán PayOS
        [HttpPost("payos/create-link/{paymentId}")]
        public async Task<IActionResult> CreatePayOSLink(int paymentId)
        {
            var userId = User.GetUserId();
            var result = await _paymentService.CreatePayOSPaymentLinkAsync(paymentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint PayOS return URL (xử lý redirect từ PayOS sau khi thanh toán)
        [HttpGet("payos/return")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] string? code,
            [FromQuery] string? desc,
            [FromQuery] string? data)
        {
            var result = await _paymentService.ProcessPayOSReturnAsync(code ?? string.Empty, desc ?? string.Empty, data ?? string.Empty);
            
            if (result.Success && result.Data != null)
            {
                return Redirect(result.Data.RedirectUrl);
            }
            
            // Fallback nếu có lỗi
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            return Redirect($"{frontendUrl}/payment-failed?reason={Uri.EscapeDataString(result.Message ?? "Server error")}");
        }

        // endpoint PayOS webhook (xử lý callback từ PayOS)
        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookDto webhookData)
        {
            var result = await _paymentService.ProcessPayOSWebhookAsync(webhookData);
            return result.Success 
                ? Ok(new { message = "Success", orderCode = webhookData.OrderCode }) 
                : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // endpoint Student xác nhận thanh toán PayOS
        [HttpPost("payos/confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayOSPayment(int paymentId)
        {
            var userId = User.GetUserId();
            var result = await _paymentService.ConfirmPayOSPaymentAsync(paymentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
