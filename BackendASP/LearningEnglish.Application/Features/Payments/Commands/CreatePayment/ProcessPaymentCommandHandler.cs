using MediatR;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Features.Payments.Events;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Features.Payments.Commands.CreatePayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ServiceResponse<CreateInforPayment>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentValidator _paymentValidator;
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;
        private readonly IMediator _mediator;

        public ProcessPaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IPaymentValidator paymentValidator,
            IEnumerable<IPaymentStrategy> paymentStrategies,
            IUnitOfWork unitOfWork,
            ILogger<ProcessPaymentCommandHandler> logger,
            IMediator mediator)
        {
            _paymentRepository = paymentRepository;
            _paymentValidator = paymentValidator;
            _paymentStrategies = paymentStrategies;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<ServiceResponse<CreateInforPayment>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var response = new ServiceResponse<CreateInforPayment>();
            var userId = request.UserId;
            var paymentRequest = request.Request;

            try
            {
                _logger.LogInformation("Bắt đầu xử lý thanh toán (Mediator) cho User {UserId}, Sản phẩm {ProductId}, Loại {TypeProduct}",
                    userId, paymentRequest.ProductId, paymentRequest.typeproduct);

                // 1. Idempotency Check
                if (!string.IsNullOrEmpty(paymentRequest.IdempotencyKey))
                {
                    var existingPayment = await _paymentRepository.GetPaymentByIdempotencyKeyAsync(userId, paymentRequest.IdempotencyKey);
                    if (existingPayment != null)
                    {
                        _logger.LogInformation("Payment idempotent hit: {IdempotencyKey}", paymentRequest.IdempotencyKey);
                        response.Success = true;
                        response.StatusCode = 200;
                        response.Message = "Payment đã được tạo trước đó";
                        response.Data = new CreateInforPayment
                        {
                            PaymentId = existingPayment.PaymentId,
                            ProductType = existingPayment.ProductType,
                            ProductId = existingPayment.ProductId,
                            Amount = existingPayment.Amount
                        };
                        return response;
                    }
                }

                // 2. Validate User
                var userValidation = await _paymentValidator.ValidateUserPaymentAsync(userId, paymentRequest.ProductId, paymentRequest.typeproduct);
                if (!userValidation.Success)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = userValidation.Message;
                    return response;
                }

                // 3. Validate Product & Get Price
                var productValidation = await _paymentValidator.ValidateProductAsync(paymentRequest.ProductId, paymentRequest.typeproduct);
                if (!productValidation.Success)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = productValidation.Message;
                    return response;
                }

                var amount = productValidation.Data;

                // 4. Get Strategy (for naming)
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == paymentRequest.typeproduct);
                if (processor == null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Loại sản phẩm không được hỗ trợ";
                    return response;
                }

                var productName = await processor.GetProductNameAsync(paymentRequest.ProductId);
                
                var baseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var random = new Random().Next(100, 999);
                var orderCode = baseTimestamp * 1000 + random;

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var payment = new Payment
                    {
                        UserId = userId,
                        ProductType = paymentRequest.typeproduct,
                        ProductId = paymentRequest.ProductId,
                        OrderCode = orderCode,
                        IdempotencyKey = string.IsNullOrEmpty(paymentRequest.IdempotencyKey) ? null : paymentRequest.IdempotencyKey,
                        Gateway = PaymentGateway.PayOs,
                        Amount = amount,
                        Status = PaymentStatus.Pending,
                        Description = $"Thanh toán {productName}",
                        CreatedAt = DateTime.UtcNow,
                        ExpiredAt = DateTime.UtcNow.AddMinutes(15),
                        ProviderTransactionId = orderCode.ToString()
                    };

                    await _paymentRepository.AddPaymentAsync(payment);
                    await _paymentRepository.SaveChangesAsync();

                    // === LOGIC MIỄN PHÍ ===
                    if (amount == 0)
                    {
                        _logger.LogInformation("Payment {PaymentId} là miễn phí. Tự động hoàn tất.", payment.PaymentId);
                        
                        payment.Status = PaymentStatus.Completed;
                        payment.PaidAt = DateTime.UtcNow;
                        payment.UpdatedAt = DateTime.UtcNow;
                        payment.Description = "Free enrollment";

                        await _paymentRepository.UpdatePaymentStatusAsync(payment);
                        await _paymentRepository.SaveChangesAsync();

                        // TRIGGER EVENT: Kích hoạt sản phẩm
                        await _mediator.Publish(new PaymentCompletedEvent(payment), cancellationToken);

                        response.Message = "Nhận sản phẩm miễn phí thành công";
                    }

                    response.Data = new CreateInforPayment
                    {
                        PaymentId = payment.PaymentId,
                        ProductId = payment.ProductId,
                        ProductType = payment.ProductType,
                        Amount = payment.Amount
                    };

                    await _unitOfWork.CommitAsync();
                    response.Success = true;
                    response.StatusCode = 200;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(ex, "Lỗi transaction khi tạo payment");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý ProcessPaymentCommand");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi xử lý thanh toán";
            }

            return response;
        }
    }
}
