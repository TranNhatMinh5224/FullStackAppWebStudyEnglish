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

        /// <summary>
        /// Tạo thông tin thanh toán (Process Payment)
        /// </summary>
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

        /// <summary>
        /// Xác nhận thanh toán (Confirm Payment)
        /// </summary>
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
    }
}
