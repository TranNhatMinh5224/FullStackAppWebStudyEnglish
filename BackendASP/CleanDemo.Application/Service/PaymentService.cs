using AutoMapper;
using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using Microsoft.Extensions.Logging;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Service
{

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentValidator _paymentValidator;
        private readonly IPaymentProcessorFactory _processorFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPaymentValidator paymentValidator,
            IPaymentProcessorFactory processorFactory,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _paymentValidator = paymentValidator;
            _processorFactory = processorFactory;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment request)
        {
            var response = new ServiceResponse<CreateInforPayment>();
            try
            {
                _logger.LogInformation("Bắt đầu xử lý thanh toán cho User {UserId}, Sản phẩm {ProductId}, Loại {TypeProduct}",
                    userId, request.ProductId, request.typeproduct);

                // 1  Validate user and check existing payment
                var userValidationResult = await _paymentValidator.ValidateUserPaymentAsync(userId, request.ProductId, request.typeproduct);
                if (!userValidationResult.Success)
                {
                    response.Success = false;
                    response.Message = userValidationResult.Message;
                    return response;
                }

                // 2 Validate product and get amount
                var productValidationResult = await _paymentValidator.ValidateProductAsync(request.ProductId, request.typeproduct);
                if (!productValidationResult.Success)
                {
                    response.Success = false;
                    response.Message = productValidationResult.Message;
                    return response;
                }

                var amount = productValidationResult.Data;

                // 4 Create paymeent use transaction
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var payment = new Payment
                    {
                        UserId = userId,
                        ProductType = request.typeproduct,
                        ProductId = request.ProductId,
                        Amount = amount,
                        Status = PaymentStatus.Pending,
                        PaidAt = null,
                        ProviderTransactionId = null
                    };

                    await _paymentRepository.AddPaymentAsync(payment);
                    await _paymentRepository.SaveChangesAsync();

                    _logger.LogInformation("Tạo thanh toán {PaymentId} thành công cho User {UserId}, Số tiền: {Amount}",
                        payment.PaymentId, userId, amount);

                    response.Data = new CreateInforPayment
                    {
                        PaymentId = payment.PaymentId,
                        ProductId = payment.ProductId,
                        ProductType = payment.ProductType,
                        Amount = payment.Amount
                    };

                    await _unitOfWork.CommitAsync();
                    _logger.LogInformation("Hoàn tất xử lý thanh toán thành công cho Payment {PaymentId}", payment.PaymentId);
                }
                catch (Exception transactionEx)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(transactionEx, "Transaction thất bại khi tạo thanh toán cho User {UserId}", userId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Xử lý thanh toán thất bại cho User {UserId}, Sản phẩm {ProductId}, Loại {TypeProduct}",
                    userId, request.ProductId, request.typeproduct);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi xử lý thanh toán";
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ConfirmPaymentAsync(CompletePayment paymentDto, int userId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var existingPayment = await _paymentRepository.GetPaymentByIdAsync(paymentDto.PaymentId);
                if (existingPayment == null)
                {
                    _logger.LogWarning("Không tìm thấy thanh toán {PaymentId}", paymentDto.PaymentId);
                    response.Success = false;
                    response.Message = "Không tìm thấy thanh toán";
                    return response;
                }

                if (existingPayment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} không có quyền truy cập thanh toán {PaymentId}", userId, paymentDto.PaymentId);
                    response.Success = false;
                    response.Message = "Không có quyền truy cập";
                    return response;
                }

                if (existingPayment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning("Thanh toán {PaymentId} đã được xử lý", paymentDto.PaymentId);
                    response.Success = false;
                    response.Message = "Thanh toán đã được xử lý";
                    return response;
                }

                await _unitOfWork.BeginTransactionAsync();

                _logger.LogInformation("Xác nhận thanh toán {PaymentId} cho User {UserId}", paymentDto.PaymentId, userId);

                // Update payment status
                existingPayment.PaymentMethod = paymentDto.PaymentMethod;
                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.PaidAt = DateTime.UtcNow;

                await _paymentRepository.UpdatePaymentStatusAsync(existingPayment);

                // xử lý thanh toán tùy theo loại sản phẩm
                try
                {
                    var processor = _processorFactory.GetProcessor(existingPayment.ProductType);
                    var postPaymentResult = await processor.ProcessPostPaymentAsync(
                        existingPayment.UserId,
                        existingPayment.ProductId,
                        paymentDto.PaymentId);

                    if (!postPaymentResult.Success)
                    {
                        _logger.LogError("Post-payment processing failed for Payment {PaymentId}: {Message}",
                            paymentDto.PaymentId, postPaymentResult.Message);
                        response.Success = false;
                        response.Message = postPaymentResult.Message;
                        await _unitOfWork.RollbackAsync();
                        return response;
                    }

                    _logger.LogInformation("Post-payment processing completed successfully for Payment {PaymentId}", paymentDto.PaymentId);
                }
                catch (NotSupportedException ex)
                {
                    _logger.LogError(ex, "Unsupported product type {ProductType} for Payment {PaymentId}",
                        existingPayment.ProductType, paymentDto.PaymentId);
                    response.Success = false;
                    response.Message = "Loại sản phẩm không được hỗ trợ";
                    await _unitOfWork.RollbackAsync();
                    return response;
                }

                await _paymentRepository.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Xác nhận thanh toán {PaymentId} thành công", paymentDto.PaymentId);
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác nhận thanh toán {PaymentId} cho User {UserId}", paymentDto.PaymentId, userId);
                await _unitOfWork.RollbackAsync();
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi xác nhận thanh toán";
                response.Data = false;
            }
            return response;
        }
    }
}