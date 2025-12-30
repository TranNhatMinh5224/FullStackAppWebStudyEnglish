using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Features.Payments.Commands.ConfirmPayment;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningEnglish.Application.Features.Payments.Commands.ProcessWebhook
{
    public class ProcessPayOSWebhookCommandHandler : IRequestHandler<ProcessPayOSWebhookCommand, ServiceResponse<bool>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly IMediator _mediator;
        private readonly ILogger<ProcessPayOSWebhookCommandHandler> _logger;

        public ProcessPayOSWebhookCommandHandler(
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            IMediator mediator,
            ILogger<ProcessPayOSWebhookCommandHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<bool>();
            var webhookData = request.WebhookData;

            try
            {
                // Verify Signature (if not skipped)
                if (!request.SkipSignatureCheck)
                {
                    var isValid = await _payOSService.VerifyWebhookSignature(webhookData.Data, webhookData.Signature);
                    if (!isValid)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Invalid signature";
                        return response;
                    }
                }

                // Get Payment
                var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(webhookData.OrderCode.ToString());
                if (payment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Payment not found";
                    return response;
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    response.Success = true;
                    response.Data = true;
                    return response;
                }

                // Check Status
                string? paymentStatus = GetStatusFromWebhook(webhookData);
                if (string.IsNullOrEmpty(paymentStatus))
                {
                    var payosInfo = await _payOSService.GetPaymentInformationAsync(webhookData.OrderCode);
                    if (payosInfo.Success && payosInfo.Data != null)
                    {
                        paymentStatus = payosInfo.Data.Status;
                    }
                }

                if (string.IsNullOrEmpty(paymentStatus) || !string.Equals(paymentStatus, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Payment not PAID";
                    return response;
                }

                // Delegate to ConfirmPaymentCommand
                var confirmDto = new CompletePayment
                {
                    PaymentId = payment.PaymentId,
                    ProductId = payment.ProductId,
                    ProductType = payment.ProductType,
                    Amount = payment.Amount,
                    PaymentMethod = PaymentGateway.PayOs.ToString()
                };

                return await _mediator.Send(new ConfirmPaymentCommand(confirmDto, payment.UserId), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = ex.Message;
                return response;
            }
        }

        private string? GetStatusFromWebhook(PayOSWebhookDto webhookData)
        {
            if (!string.IsNullOrEmpty(webhookData.Status)) return webhookData.Status;
            
            try
            {
                if (!string.IsNullOrEmpty(webhookData.Data))
                {
                    var doc = JsonDocument.Parse(webhookData.Data);
                    if (doc.RootElement.TryGetProperty("status", out var statusEl))
                        return statusEl.GetString();
                }
            }
            catch { }
            return null;
        }
    }
}
