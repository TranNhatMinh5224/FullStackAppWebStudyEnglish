using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using Microsoft.Extensions.Configuration;
using LearningEnglish.Application.Common.Pagination;
using Microsoft.Extensions.Logging;
using LearningEnglish.Application.Features.Payments.Commands.CreatePayment;
using LearningEnglish.Application.Features.Payments.Commands.ConfirmPayment;
using LearningEnglish.Application.Features.Payments.Queries.GetTransactionHistory;
using LearningEnglish.Application.Features.Payments.Queries.GetTransactionDetail;
using LearningEnglish.Application.Features.Payments.Commands.CreatePayOSLink;
using LearningEnglish.Application.Features.Payments.Commands.ProcessPayOSReturn;
using LearningEnglish.Application.Features.Payments.Commands.ProcessWebhook;
using LearningEnglish.Application.Features.Payments.Commands.ConfirmPayOSPayment;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/payments")]
    [Authorize(Roles = "Student,Teacher")]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IMediator mediator,
            IConfiguration configuration,
            ILogger<PaymentController> logger)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

       
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] requestPayment request)
        {
            var userId = User.GetUserId();
            var command = new ProcessPaymentCommand(userId, request);
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student xác nhận thanh toán
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] CompletePayment paymentDto)
        {
            var userId = User.GetUserId();
            var command = new ConfirmPaymentCommand(paymentDto, userId);
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student lấy lịch sử giao dịch (phân trang)
        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] PageRequest request)
        {
            var userId = User.GetUserId();
            var query = new GetTransactionHistoryQuery(userId, request);
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student lấy chi tiết giao dịch theo paymentId
        [HttpGet("transaction/{paymentId}")]
        public async Task<IActionResult> GetTransactionDetail(int paymentId)
        {
            var userId = User.GetUserId();
            var query = new GetTransactionDetailQuery(paymentId, userId);
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Student tạo link thanh toán PayOS
        [HttpPost("payos/create-link/{paymentId}")]
        public async Task<IActionResult> CreatePayOSLink(int paymentId)
        {
            var userId = User.GetUserId();
            var command = new CreatePayOSLinkCommand(paymentId, userId);
            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                if (result.Message?.Contains("PayOS error") == true || 
                    result.Message?.Contains("PayOS") == true)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = result.Message,
                        statusCode = 400 
                    });
                }
                return StatusCode(result.StatusCode, result);
            }
            
            return Ok(result);
        }

        // endpoint PayOS return URL
        [HttpGet("payos/return")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] string? code,
            [FromQuery] string? desc,
            [FromQuery] string? data,
            [FromQuery] long? orderCode,
            [FromQuery] bool? cancel,
            [FromQuery] string? status,
            [FromQuery] string? signature)
        {
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";

            if (cancel == true || string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Payment cancelled: OrderCode={OrderCode}", orderCode);
                return Redirect($"{frontendUrl}/payment-failed?reason=cancelled&orderCode={orderCode}");
            }

            if (!string.IsNullOrEmpty(status) && !string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Payment not yet paid: OrderCode={OrderCode}", orderCode);
                return Redirect($"{frontendUrl}/payment-pending?orderCode={orderCode}&status={Uri.EscapeDataString(status ?? "")}");
            }

            var command = new ProcessPayOSReturnCommand(
                code ?? string.Empty, 
                desc ?? string.Empty, 
                data ?? string.Empty, 
                orderCode?.ToString(), 
                status);

            var result = await _mediator.Send(command);
            
            if (result.Success && result.Data != null && !string.IsNullOrEmpty(result.Data.RedirectUrl))
            {
                return Redirect(result.Data.RedirectUrl);
            }
            
            return Redirect($"{frontendUrl}/payment-failed?reason={Uri.EscapeDataString(result.Message ?? "Server error")}");
        }

        // endpoint PayOS cancel URL
        [HttpGet("payos/cancel")]
        [AllowAnonymous]
        public IActionResult PayOSCancel([FromQuery] long? orderCode)
        {
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            _logger.LogInformation("Payment cancelled by user: OrderCode={OrderCode}", orderCode);
            return Redirect($"{frontendUrl}/payment-failed?reason=cancelled&orderCode={orderCode}");
        }

        // endpoint PayOS webhook
        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookDto webhookData)
        {
            _logger.LogInformation("PayOS Webhook: OrderCode={OrderCode}", webhookData.OrderCode);
            
            var command = new ProcessPayOSWebhookCommand(webhookData);
            var result = await _mediator.Send(command);
            
            return result.Success 
                ? Ok(new { message = "Success", orderCode = webhookData.OrderCode }) 
                : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // endpoint Student xác nhận thanh toán PayOS
        [HttpPost("payos/confirm/{paymentId}")]
        public async Task<IActionResult> ConfirmPayOSPayment(int paymentId)
        {
            var userId = User.GetUserId();
            var command = new ConfirmPayOSPaymentCommand(paymentId, userId);
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}