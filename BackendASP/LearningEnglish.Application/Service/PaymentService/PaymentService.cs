using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Service
{

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentValidator _paymentValidator;
        private readonly IEnumerable<IPaymentStrategy> _paymentStrategies;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayOSService _payOSService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPaymentValidator paymentValidator,
            IEnumerable<IPaymentStrategy> paymentStrategies,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IUnitOfWork unitOfWork,
            IPayOSService payOSService)
        {
            _paymentRepository = paymentRepository;
            _paymentValidator = paymentValidator;
            _paymentStrategies = paymentStrategies;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _payOSService = payOSService;
        }
        // POST /api/payments - Create Payment
        // service tao process payment

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
                    response.StatusCode = 400; // Bad Request
                    response.Message = userValidationResult.Message;
                    return response;
                }

                // 2 Validate product and get amount
                var productValidationResult = await _paymentValidator.ValidateProductAsync(request.ProductId, request.typeproduct);
                if (!productValidationResult.Success)
                {
                    response.Success = false;
                    response.StatusCode = 404; // Not Found (product không tồn tại)
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

                    // Nếu amount = 0 (miễn phí), tự động confirm ngay
                    if (amount == 0)
                    {
                        _logger.LogInformation("Payment {PaymentId} có amount = 0, tự động confirm miễn phí", payment.PaymentId);

                        payment.Status = PaymentStatus.Completed;
                        payment.PaidAt = DateTime.UtcNow;
                        payment.PaymentMethod = "Free";
                        await _paymentRepository.UpdatePaymentStatusAsync(payment);
                        await _paymentRepository.SaveChangesAsync();

                        // Kích hoạt sản phẩm ngay
                        var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                        if (processor == null)
                        {
                            _logger.LogError("No payment strategy found for product type {ProductType}", payment.ProductType);
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Loại sản phẩm không được hỗ trợ";
                            await _unitOfWork.RollbackAsync();
                            return response;
                        }

                        var postPaymentResult = await processor.ProcessPostPaymentAsync(
                            payment.UserId,
                            payment.ProductId,
                            payment.PaymentId);

                        if (!postPaymentResult.Success)
                        {
                            _logger.LogError("Post-payment processing failed for free Payment {PaymentId}: {Message}",
                                payment.PaymentId, postPaymentResult.Message);
                            response.Success = false;
                            response.StatusCode = 500;
                            response.Message = postPaymentResult.Message;
                            await _unitOfWork.RollbackAsync();
                            return response;
                        }

                        _logger.LogInformation("Sản phẩm miễn phí đã được kích hoạt thành công cho Payment {PaymentId}", payment.PaymentId);
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
                response.StatusCode = 500; // Internal Server Error
                response.Message = "Đã xảy ra lỗi khi xử lý thanh toán";
                return response;
            }
            response.StatusCode = 200; // Success
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
                    response.StatusCode = 404; // Not Found
                    response.Message = "Không tìm thấy thanh toán";
                    return response;
                }

                if (existingPayment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} không có quyền truy cập thanh toán {PaymentId}", userId, paymentDto.PaymentId);
                    response.Success = false;
                    response.StatusCode = 403; // Forbidden
                    response.Message = "Không có quyền truy cập";
                    return response;
                }

                if (existingPayment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning("Thanh toán {PaymentId} đã được xử lý", paymentDto.PaymentId);
                    response.Success = false;
                    response.StatusCode = 400; // Bad Request
                    response.Message = "Thanh toán đã được xử lý";
                    return response;
                }

                // Validate Amount from DB - Security check to prevent client manipulation
                if (existingPayment.Amount != paymentDto.Amount)
                {
                    _logger.LogWarning("Amount mismatch cho Payment {PaymentId}. Expected: {ExpectedAmount}, Received: {ReceivedAmount}",
                        paymentDto.PaymentId, existingPayment.Amount, paymentDto.Amount);
                    response.Success = false;
                    response.StatusCode = 400; // Bad Request
                    response.Message = "Số tiền thanh toán không khớp";
                    return response;
                }

                // Validate ProductId and ProductType from DB
                if (existingPayment.ProductId != paymentDto.ProductId || existingPayment.ProductType != paymentDto.ProductType)
                {
                    _logger.LogWarning("Product mismatch cho Payment {PaymentId}. Expected: {ExpectedProduct}/{ExpectedType}, Received: {ReceivedProduct}/{ReceivedType}",
                        paymentDto.PaymentId, existingPayment.ProductId, existingPayment.ProductType, paymentDto.ProductId, paymentDto.ProductType);
                    response.Success = false;
                    response.StatusCode = 400; // Bad Request
                    response.Message = "Thông tin sản phẩm không khớp";
                    return response;
                }

                await _unitOfWork.BeginTransactionAsync();

                _logger.LogInformation("Xác nhận thanh toán {PaymentId} cho User {UserId}", paymentDto.PaymentId, userId);


                _logger.LogInformation("Cập nhật payment status thành Completed cho Payment {PaymentId}", paymentDto.PaymentId);
                existingPayment.PaymentMethod = paymentDto.PaymentMethod;
                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.PaidAt = DateTime.UtcNow;

                await _paymentRepository.UpdatePaymentStatusAsync(existingPayment);
                await _paymentRepository.SaveChangesAsync();


                try
                {
                    var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == existingPayment.ProductType);
                    if (processor == null)
                    {
                        _logger.LogError("No payment strategy found for product type {ProductType}", existingPayment.ProductType);
                        response.Success = false;
                        response.StatusCode = 400; // Bad Request
                        response.Message = "Loại sản phẩm không được hỗ trợ";
                        await _unitOfWork.RollbackAsync();
                        return response;
                    }

                    var postPaymentResult = await processor.ProcessPostPaymentAsync(
                        existingPayment.UserId,
                        existingPayment.ProductId,
                        paymentDto.PaymentId);

                    if (!postPaymentResult.Success)
                    {
                        _logger.LogError("Post-payment processing failed for Payment {PaymentId}: {Message}",
                            paymentDto.PaymentId, postPaymentResult.Message);
                        response.Success = false;
                        response.StatusCode = 500; // Internal Server Error
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
                    response.StatusCode = 400; // Bad Request
                    response.Message = "Loại sản phẩm không được hỗ trợ";
                    await _unitOfWork.RollbackAsync();
                    return response;
                }

                // Payment status đã được update ở trên, chỉ cần commit transaction
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Xác nhận thanh toán {PaymentId} thành công", paymentDto.PaymentId);
                response.StatusCode = 200; // Success
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác nhận thanh toán {PaymentId} cho User {UserId}", paymentDto.PaymentId, userId);
                await _unitOfWork.RollbackAsync();
                response.Success = false;
                response.StatusCode = 500; // Internal Server Error
                response.Message = "Đã xảy ra lỗi khi xác nhận thanh toán. Vui lòng thử lại sau.";
                response.Data = false;
            }
            return response;
        }
        // Service laays ra thong tin lich su giao dich (Phân trang)
        public async Task<ServiceResponse<PagedResult<TransactionHistoryDto>>> GetTransactionHistoryAsync(int userId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<TransactionHistoryDto>>();
            try
            {
                _logger.LogInformation("Getting transaction history for User {UserId}, Page {PageNumber}, Size {PageSize}",
                    userId, request.PageNumber, request.PageSize);

                var totalCount = await _paymentRepository.GetTransactionCountAsync(userId);
                var payments = await _paymentRepository.GetTransactionHistoryAsync(userId, request.PageNumber, request.PageSize);

                var transactionDtos = new List<TransactionHistoryDto>();
                foreach (var payment in payments)
                {
                    var productName = await GetProductNameAsync(payment.ProductId, payment.ProductType);

                    transactionDtos.Add(new TransactionHistoryDto
                    {
                        PaymentId = payment.PaymentId,
                        PaymentMethod = payment.PaymentMethod ?? "N/A",
                        ProductType = payment.ProductType,
                        ProductId = payment.ProductId,
                        ProductName = productName,
                        Amount = payment.Amount,
                        Status = payment.Status,
                        CreatedAt = payment.PaidAt ?? DateTime.UtcNow,
                        PaidAt = payment.PaidAt,
                        ProviderTransactionId = payment.ProviderTransactionId
                    });
                }

                response.Data = new PagedResult<TransactionHistoryDto>
                {
                    Items = transactionDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                _logger.LogInformation("Retrieved {Count} transactions for User {UserId}", transactionDtos.Count, userId);
                response.StatusCode = 200; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction history for User {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500; // Internal Server Error
                response.Message = "Đã xảy ra lỗi khi lấy lịch sử giao dịch";
            }
            return response;
        }
        // Service lay toan bo lich su giao dich (Khong phan trang)
        public async Task<ServiceResponse<List<TransactionHistoryDto>>> GetAllTransactionHistoryAsync(int userId)
        {
            var response = new ServiceResponse<List<TransactionHistoryDto>>();
            try
            {
                _logger.LogInformation("Getting all transaction history for User {UserId}", userId);

                var payments = await _paymentRepository.GetAllTransactionHistoryAsync(userId);

                var transactionDtos = new List<TransactionHistoryDto>();
                foreach (var payment in payments)
                {
                    var productName = await GetProductNameAsync(payment.ProductId, payment.ProductType);

                    transactionDtos.Add(new TransactionHistoryDto
                    {
                        PaymentId = payment.PaymentId,
                        PaymentMethod = payment.PaymentMethod ?? "N/A",
                        ProductType = payment.ProductType,
                        ProductId = payment.ProductId,
                        ProductName = productName,
                        Amount = payment.Amount,
                        Status = payment.Status,
                        CreatedAt = payment.PaidAt ?? DateTime.UtcNow,
                        PaidAt = payment.PaidAt,
                        ProviderTransactionId = payment.ProviderTransactionId
                    });
                }

                response.Data = transactionDtos;
                _logger.LogInformation("Retrieved {Count} transactions for User {UserId}", transactionDtos.Count, userId);
                response.StatusCode = 200; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all transaction history for User {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500; // Internal Server Error
                response.Message = "Đã xảy ra lỗi khi lấy lịch sử giao dịch";
            }
            return response;
        }

        // service lay chi tiet giao dich

        public async Task<ServiceResponse<TransactionDetailDto>> GetTransactionDetailAsync(int paymentId, int userId)
        {
            var response = new ServiceResponse<TransactionDetailDto>();
            try
            {
                _logger.LogInformation("Getting transaction detail for Payment {PaymentId}, User {UserId}", paymentId, userId);

                var payment = await _paymentRepository.GetTransactionDetailAsync(paymentId, userId);
                if (payment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404; // Not Found
                    response.Message = "Không tìm thấy giao dịch";
                    return response;
                }

                var productName = await GetProductNameAsync(payment.ProductId, payment.ProductType);

                response.Data = new TransactionDetailDto
                {
                    PaymentId = payment.PaymentId,
                    UserId = payment.UserId,
                    UserName = payment.User != null ? $"{payment.User.FirstName} {payment.User.LastName}" : "N/A",
                    UserEmail = payment.User?.Email ?? "N/A",
                    PaymentMethod = payment.PaymentMethod ?? "N/A",
                    ProductType = payment.ProductType,
                    ProductId = payment.ProductId,
                    ProductName = productName,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    CreatedAt = payment.PaidAt ?? DateTime.UtcNow,
                    PaidAt = payment.PaidAt,
                    ProviderTransactionId = payment.ProviderTransactionId
                };

                _logger.LogInformation("Retrieved transaction detail for Payment {PaymentId}", paymentId);
                response.StatusCode = 200; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction detail for Payment {PaymentId}", paymentId);
                response.Success = false;
                response.StatusCode = 500; // Internal Server Error
                response.Message = "Đã xảy ra lỗi khi lấy chi tiết giao dịch";
            }
            return response;
        }
        public async Task<ServiceResponse<PayOSLinkResponse>> CreatePayOSPaymentLinkAsync(int paymentId, int userId)
        {
            var response = new ServiceResponse<PayOSLinkResponse>();
            try
            {
                _logger.LogInformation("Creating PayOS payment link for Payment {PaymentId}, User {UserId}", paymentId, userId);

                // 1. Validate payment
                var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for User {UserId}", paymentId, userId);
                    response.Success = false;
                    response.StatusCode = 404; // Not Found
                    response.Message = "Payment not found";
                    return response;
                }

                if (payment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} access denied for Payment {PaymentId}", userId, paymentId);
                    response.Success = false;
                    response.StatusCode = 403; // Forbidden
                    response.Message = "Access denied";
                    return response;
                }

                if (payment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning("Payment {PaymentId} already processed with status {Status}", paymentId, payment.Status);
                    response.Success = false;
                    response.StatusCode = 400; // Bad Request
                    response.Message = "Payment already processed";
                    return response;
                }

                // 2. Get product name
                var productName = await GetProductNameAsync(payment.ProductId, payment.ProductType);
                var description = $"Thanh toán {productName}";

                // 3. Create PayOS link
                var linkRequest = new CreatePayOSLinkRequest
                {
                    PaymentId = payment.PaymentId
                };

                var linkResponse = await _payOSService.CreatePaymentLinkAsync(
                    linkRequest, 
                    payment.Amount, 
                    productName, 
                    description);

                if (!linkResponse.Success || linkResponse.Data == null)
                {
                    _logger.LogError("Failed to create PayOS link: {Message}", linkResponse.Message);
                    response.Success = false;
                    response.StatusCode = 500; // Internal Server Error (PayOS API error)
                    response.Message = linkResponse.Message ?? "Failed to create PayOS payment link";
                    return response;
                }

                // 4. Update payment with orderCode
                payment.ProviderTransactionId = linkResponse.Data.OrderCode;
                await _paymentRepository.UpdatePaymentStatusAsync(payment);
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("PayOS payment link created successfully for Payment {PaymentId}, OrderCode: {OrderCode}",
                    paymentId, linkResponse.Data.OrderCode);

                response.Data = linkResponse.Data;
                response.Success = true;
                response.StatusCode = 200; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS link for Payment {PaymentId}", paymentId);
                response.Success = false;
                response.StatusCode = 500; // Internal Server Error
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        // Lấy tên sản phẩm dựa trên loại và ID sản phẩm
        private static Task<string> GetProductNameAsync(int productId, ProductType productType)
        {
            // Return generic product name based on type
            // For full product details, query Course/TeacherPackage repositories separately
            var productName = productType switch
            {
                ProductType.Course => $"Khóa học #{productId}",
                ProductType.TeacherPackage => $"Gói giáo viên #{productId}",
                _ => "Sản phẩm"
            };

            return Task.FromResult(productName);
        }
    }
}
