using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LearningEnglish.Application.Validators.Payment;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/payment")]
    [Authorize(Roles = "Student,User")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly RequestPaymentValidator _requestValidator;
        private readonly CompletePaymentValidator _completeValidator;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger,
            RequestPaymentValidator requestValidator,
            CompletePaymentValidator completeValidator)
        {
            _paymentService = paymentService;
            _logger = logger;
            _requestValidator = requestValidator;
            _completeValidator = completeValidator;
        }

        // Tạo thông tin thanh toán (Process Payment)
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] requestPayment request)
        {
            // Validate input
            var validationResult = await _requestValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await _paymentService.ProcessPaymentAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        // Xác nhận thanh toán (Confirm Payment)
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] CompletePayment paymentDto)
        {
            // Validate input
            var validationResult = await _completeValidator.ValidateAsync(paymentDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await _paymentService.ConfirmPaymentAsync(paymentDto, userId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Payment confirmed successfully" });
        }

        /// <summary>
        /// Get transaction history for authenticated user
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                if (pageNumber < 1)
                {
                    return BadRequest(new { message = "Page number must be greater than 0" });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new { message = "Page size must be between 1 and 100" });
                }

                var result = await _paymentService.GetTransactionHistoryAsync(userId, pageNumber, pageSize);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction history");
                return StatusCode(500, new { message = "An error occurred while retrieving transaction history" });
            }
        }

        /// <summary>
        /// Get transaction detail by payment ID
        /// </summary>
        [HttpGet("transaction/{paymentId}")]
        public async Task<IActionResult> GetTransactionDetail(int paymentId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                var result = await _paymentService.GetTransactionDetailAsync(paymentId, userId);
                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction detail for Payment {PaymentId}", paymentId);
                return StatusCode(500, new { message = "An error occurred while retrieving transaction detail" });
            }
        }
    }
}
