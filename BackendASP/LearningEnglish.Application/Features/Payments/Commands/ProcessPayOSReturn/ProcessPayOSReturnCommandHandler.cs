using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Features.Payments.Commands.ConfirmPayment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LearningEnglish.Application.Features.Payments.Commands.ProcessPayOSReturn
{
    public class ProcessPayOSReturnCommandHandler : IRequestHandler<ProcessPayOSReturnCommand, ServiceResponse<PayOSReturnResult>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProcessPayOSReturnCommandHandler> _logger;

        public ProcessPayOSReturnCommandHandler(
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            IMediator mediator,
            IConfiguration configuration,
            ILogger<ProcessPayOSReturnCommandHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResponse<PayOSReturnResult>> Handle(ProcessPayOSReturnCommand request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<PayOSReturnResult>();
            var result = new PayOSReturnResult();
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";

            try
            {
                if (request.Code != "00")
                {
                    result.Success = false;
                    result.RedirectUrl = $"{frontendUrl}/payment-failed?reason={Uri.EscapeDataString(request.Desc ?? "Failed")}";
                    response.Data = result;
                    return response;
                }

                Payment? payment = null;
                string? finalOrderCode = request.OrderCode;

                // Try to find payment
                if (!string.IsNullOrEmpty(request.Data))
                {
                    try {
                        var doc = JsonDocument.Parse(request.Data);
                        if (doc.RootElement.TryGetProperty("orderCode", out var oc))
                        {
                            finalOrderCode = oc.GetInt64().ToString();
                            payment = await _paymentRepository.GetPaymentByTransactionIdAsync(finalOrderCode);
                        }
                    } catch { }
                }

                if (payment == null && !string.IsNullOrEmpty(finalOrderCode) && long.TryParse(finalOrderCode, out var codeLong))
                {
                    payment = await _paymentRepository.GetPaymentByOrderCodeAsync(codeLong);
                }

                if (payment == null)
                {
                    result.Success = false;
                    result.RedirectUrl = $"{frontendUrl}/payment-failed?reason=Payment not found";
                    response.Data = result;
                    return response;
                }

                // Check Status from PayOS
                string? status = request.Status;
                if (string.IsNullOrEmpty(status))
                {
                    if (long.TryParse(finalOrderCode, out var c))
                    {
                        var info = await _payOSService.GetPaymentInformationAsync(c);
                        if (info.Success && info.Data != null) status = info.Data.Status;
                    }
                }

                if (!string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.RedirectUrl = $"{frontendUrl}/payment-pending?orderCode={finalOrderCode}";
                    response.Data = result;
                    return response;
                }

                // Auto Confirm
                if (payment.Status == PaymentStatus.Pending)
                {
                    var confirmDto = new CompletePayment
                    {
                        PaymentId = payment.PaymentId,
                        ProductId = payment.ProductId,
                        ProductType = payment.ProductType,
                        Amount = payment.Amount,
                        PaymentMethod = PaymentGateway.PayOs.ToString()
                    };

                    await _mediator.Send(new ConfirmPaymentCommand(confirmDto, payment.UserId), cancellationToken);
                }

                // Redirect
                result.Success = true;
                result.RedirectUrl = payment.ProductType == ProductType.Course 
                    ? $"{frontendUrl}/course/{payment.ProductId}" 
                    : $"{frontendUrl}/home";

                response.Success = true;
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing return");
                response.Success = false;
                result.RedirectUrl = $"{frontendUrl}/payment-failed?reason=Server Error";
                response.Data = result;
                return response;
            }
        }
    }
}
