using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Features.Payments.Commands.CreatePayOSLink
{
    public class CreatePayOSLinkCommandHandler : IRequestHandler<CreatePayOSLinkCommand, ServiceResponse<PayOSLinkResponse>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;
        private readonly ILogger<CreatePayOSLinkCommandHandler> _logger;

        public CreatePayOSLinkCommandHandler(
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            IEnumerable<IPaymentStrategy> paymentStrategies,
            ILogger<CreatePayOSLinkCommandHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _paymentStrategies = paymentStrategies;
            _logger = logger;
        }

        public async Task<ServiceResponse<PayOSLinkResponse>> Handle(CreatePayOSLinkCommand request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<PayOSLinkResponse>();
            var paymentId = request.PaymentId;
            var userId = request.UserId;

            try
            {
                var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
                
                // Validate exist & owner
                if (payment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy thanh toán";
                    return response;
                }

                if (payment.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền truy cập thanh toán này";
                    return response;
                }

                if (payment.Status != PaymentStatus.Pending)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Thanh toán đã được xử lý";
                    return response;
                }

                // Validate PayOS requirements
                if (payment.OrderCode == 0)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Missing OrderCode";
                    return response;
                }

                if (payment.Gateway != PaymentGateway.PayOs)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Gateway is not PayOS";
                    return response;
                }

                // Get Product Name
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                var productName = processor != null 
                    ? await processor.GetProductNameAsync(payment.ProductId)
                    : "Sản phẩm";
                
                var description = !string.IsNullOrEmpty(payment.Description) 
                    ? payment.Description 
                    : productName;

                var linkRequest = new CreatePayOSLinkRequest { PaymentId = payment.PaymentId };

                // Call PayOS Service
                var linkResponse = await _payOSService.CreatePaymentLinkAsync(
                    linkRequest, 
                    payment.Amount, 
                    productName, 
                    description,
                    payment.OrderCode);

                // Retry logic for OrderCode conflict (Code 231)
                if (!linkResponse.Success && linkResponse.Message?.Contains("231") == true)
                {
                    _logger.LogWarning("OrderCode conflict for Payment {Id}. Generating new one.", paymentId);
                    var newOrderCode = (long)paymentId * 1000000000L + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    
                    payment.OrderCode = newOrderCode;
                    payment.ProviderTransactionId = newOrderCode.ToString();
                    await _paymentRepository.UpdatePaymentStatusAsync(payment);
                    await _paymentRepository.SaveChangesAsync();

                    linkResponse = await _payOSService.CreatePaymentLinkAsync(
                        linkRequest, 
                        payment.Amount, 
                        productName, 
                        description,
                        newOrderCode);
                }

                if (!linkResponse.Success || linkResponse.Data == null)
                {
                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = linkResponse.Message ?? "Failed to create PayOS link";
                    return response;
                }

                // Update Payment with CheckoutUrl
                payment.CheckoutUrl = linkResponse.Data.CheckoutUrl;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdatePaymentStatusAsync(payment);
                await _paymentRepository.SaveChangesAsync();

                response.Data = linkResponse.Data;
                response.Success = true;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS link (Mediator)");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
