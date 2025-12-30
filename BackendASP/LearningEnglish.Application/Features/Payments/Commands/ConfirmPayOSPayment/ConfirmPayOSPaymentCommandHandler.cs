using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Features.Payments.Commands.ConfirmPayment;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Features.Payments.Commands.ConfirmPayOSPayment
{
    public class ConfirmPayOSPaymentCommandHandler : IRequestHandler<ConfirmPayOSPaymentCommand, ServiceResponse<bool>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly IMediator _mediator;
        private readonly ILogger<ConfirmPayOSPaymentCommandHandler> _logger;

        public ConfirmPayOSPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            IMediator mediator,
            ILogger<ConfirmPayOSPaymentCommandHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> Handle(ConfirmPayOSPaymentCommand request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<bool>();
            var paymentId = request.PaymentId;
            var userId = request.UserId;

            try
            {
                var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Payment not found";
                    return response;
                }

                if (payment.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Forbidden";
                    return response;
                }

                // Verify PayOS status
                if (!string.IsNullOrEmpty(payment.ProviderTransactionId) &&
                    long.TryParse(payment.ProviderTransactionId, out var orderCode))
                {
                    var payosInfo = await _payOSService.GetPaymentInformationAsync(orderCode);
                    if (!payosInfo.Success || payosInfo.Data == null || payosInfo.Data.Code != "00")
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Payment not completed on PayOS";
                        return response;
                    }

                    if (string.IsNullOrEmpty(payosInfo.Data.Status) || !string.Equals(payosInfo.Data.Status, "PAID", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Payment status is not PAID";
                        return response;
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

                return await _mediator.Send(new ConfirmPaymentCommand(confirmDto, userId), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming PayOS payment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}
