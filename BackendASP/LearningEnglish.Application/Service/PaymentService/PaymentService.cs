using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using System.Text.Json;

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

                // 1. Check IdempotencyKey for duplicate request prevention
                if (!string.IsNullOrEmpty(request.IdempotencyKey))
                {
                    var existingPayment = await _paymentRepository.GetPaymentByIdempotencyKeyAsync(userId, request.IdempotencyKey);
                    if (existingPayment != null)
                    {
                        _logger.LogInformation("Payment với IdempotencyKey {IdempotencyKey} đã tồn tại (PaymentId: {PaymentId}), trả về payment hiện có",
                            request.IdempotencyKey, existingPayment.PaymentId);
                        
                        response.Success = true;
                        response.StatusCode = 200;
                        response.Message = "Payment đã được tạo trước đó (idempotent request)";
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

                // 2. Validate user and check existing payment
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

                // Get processor for product name
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == request.typeproduct);
                if (processor == null)
                {
                    _logger.LogError("No payment strategy found for product type {ProductType}", request.typeproduct);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Loại sản phẩm không được hỗ trợ";
                    return response;
                }

                // Generate unique OrderCode for PayOS
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var orderCode = long.Parse($"{timestamp}{request.ProductId:D6}");
                
                // Get product name for description from Strategy
                var productName = await processor.GetProductNameAsync(request.ProductId);

                // 4 Create payment use transaction
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var payment = new Payment
                    {
                        UserId = userId,
                        ProductType = request.typeproduct,
                        ProductId = request.ProductId,
                        OrderCode = orderCode,
                        IdempotencyKey = string.IsNullOrEmpty(request.IdempotencyKey) ? null : request.IdempotencyKey,
                        Gateway = PaymentGateway.PayOs,
                        Amount = amount,
                        Status = PaymentStatus.Pending,
                        Description = $"Thanh toán {productName}",
                        CreatedAt = DateTime.UtcNow,
                        ExpiredAt = DateTime.UtcNow.AddMinutes(15),
                        PaidAt = null,
                        ProviderTransactionId = orderCode.ToString()
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
                        payment.UpdatedAt = DateTime.UtcNow;
                        payment.Description = "Free course enrollment";
                        await _paymentRepository.UpdatePaymentStatusAsync(payment);
                        await _paymentRepository.SaveChangesAsync();

                        // Kích hoạt sản phẩm ngay (reuse processor from above)
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
                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.PaidAt = DateTime.UtcNow;
                existingPayment.UpdatedAt = DateTime.UtcNow;

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
                    var dto = _mapper.Map<TransactionHistoryDto>(payment);
                    
                    // Get product name from Strategy
                    var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                    dto.ProductName = processor != null 
                        ? await processor.GetProductNameAsync(payment.ProductId)
                        : "Sản phẩm";
                    
                    transactionDtos.Add(dto);
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

                var dto = _mapper.Map<TransactionDetailDto>(payment);
                
                // Get product name from Strategy
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                dto.ProductName = processor != null 
                    ? await processor.GetProductNameAsync(payment.ProductId)
                    : "Sản phẩm";

                response.Data = dto;

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
                    response.StatusCode = 404;
                    response.Message = "Payment not found";
                    return response;
                }

                if (payment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} access denied for Payment {PaymentId}", userId, paymentId);
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Access denied";
                    return response;
                }

                if (payment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning("Payment {PaymentId} already processed with status {Status}", paymentId, payment.Status);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Payment already processed";
                    return response;
                }

                // 2. Validate PayOS specific requirements
                if (payment.OrderCode == 0)
                {
                    _logger.LogError("Payment {PaymentId} has no OrderCode", paymentId);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Payment OrderCode is missing";
                    return response;
                }

                if (payment.Gateway != PaymentGateway.PayOs)
                {
                    _logger.LogWarning("Payment {PaymentId} gateway is {Gateway}, not PayOS", paymentId, payment.Gateway);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Payment is not configured for PayOS gateway";
                    return response;
                }

                if (payment.ExpiredAt.HasValue && payment.ExpiredAt.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Payment {PaymentId} has expired at {ExpiredAt}", paymentId, payment.ExpiredAt);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Payment link has expired";
                    return response;
                }

                // 3. Get product name for PayOS from Strategy
                var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == payment.ProductType);
                var productName = processor != null 
                    ? await processor.GetProductNameAsync(payment.ProductId)
                    : "Sản phẩm";
                var description = payment.Description ?? $"Thanh toán {productName}";

                // 4. Create PayOS link using existing OrderCode
                var linkRequest = new CreatePayOSLinkRequest
                {
                    PaymentId = payment.PaymentId
                };

                var linkResponse = await _payOSService.CreatePaymentLinkAsync(
                    linkRequest, 
                    payment.Amount, 
                    productName, 
                    description,
                    payment.OrderCode); // Pass existing OrderCode

                if (!linkResponse.Success || linkResponse.Data == null)
                {
                    _logger.LogError("Failed to create PayOS link: {Message}", linkResponse.Message);
                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = linkResponse.Message ?? "Failed to create PayOS payment link";
                    return response;
                }

                // 5. Update payment with CheckoutUrl
                payment.CheckoutUrl = linkResponse.Data.CheckoutUrl;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdatePaymentStatusAsync(payment);
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("PayOS payment link created successfully for Payment {PaymentId}, OrderCode: {OrderCode}",
                    paymentId, payment.OrderCode);

                response.Data = linkResponse.Data;
                response.Success = true;
                response.StatusCode = 200;
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

        // Process webhook from queue (used by retry mechanism)
        public async Task<ServiceResponse<bool>> ProcessWebhookFromQueueAsync(PayOSWebhookDto webhookData)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                _logger.LogInformation("Processing webhook from queue for OrderCode {OrderCode}", webhookData.OrderCode);

                // Find payment by OrderCode
                var payment = await _paymentRepository.GetPaymentByOrderCodeAsync(webhookData.OrderCode);
                
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for OrderCode {OrderCode}", webhookData.OrderCode);
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Payment not found";
                    return response;
                }

                // Check if already processed
                if (payment.Status == PaymentStatus.Completed)
                {
                    _logger.LogInformation("Payment {PaymentId} already completed", payment.PaymentId);
                    response.Success = true;
                    response.Data = true;
                    response.Message = "Payment already processed";
                    return response;
                }

                // Check payment status from webhook
                if (webhookData.Code != "00")
                {
                    _logger.LogWarning("Webhook indicates payment not successful: Code={Code}, Desc={Desc}",
                        webhookData.Code, webhookData.Desc);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Payment failed: {webhookData.Desc}";
                    return response;
                }

                // Create CompletePayment DTO and confirm
                var confirmDto = new CompletePayment
                {
                    PaymentId = payment.PaymentId,
                    ProductId = payment.ProductId,
                    ProductType = payment.ProductType,
                    Amount = payment.Amount,
                    PaymentMethod = PaymentGateway.PayOs.ToString()
                };

                var confirmResult = await ConfirmPaymentAsync(confirmDto, payment.UserId);

                response.Success = confirmResult.Success;
                response.StatusCode = confirmResult.StatusCode;
                response.Message = confirmResult.Message;
                response.Data = confirmResult.Data;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook from queue");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        // Xử lý PayOS return URL
        public async Task<ServiceResponse<PayOSReturnResult>> ProcessPayOSReturnAsync(string code, string desc, string data)
        {
            var response = new ServiceResponse<PayOSReturnResult>();
            var result = new PayOSReturnResult();

            try
            {
                _logger.LogInformation("Processing PayOS return: code={Code}", code);

                // Validate return parameters
                if (code != "00")
                {
                    _logger.LogWarning("PayOS payment failed: {Desc}", desc);
                    result.Success = false;
                    result.RedirectUrl = $"{GetFrontendUrl()}/payment-failed?reason={Uri.EscapeDataString(desc ?? "Payment failed")}";
                    result.Message = desc ?? "Payment failed";
                    response.Data = result;
                    return response;
                }

                if (string.IsNullOrEmpty(data))
                {
                    _logger.LogWarning("PayOS return data is empty");
                    result.Success = false;
                    result.RedirectUrl = $"{GetFrontendUrl()}/payment-failed?reason=Invalid data";
                    result.Message = "Invalid data";
                    response.Data = result;
                    return response;
                }

                // Parse return data
                JsonElement webhookData;
                try
                {
                    webhookData = JsonSerializer.Deserialize<JsonElement>(data);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing PayOS return data");
                    result.Success = false;
                    result.RedirectUrl = $"{GetFrontendUrl()}/payment-failed?reason=Invalid data format";
                    result.Message = "Invalid data format";
                    response.Data = result;
                    return response;
                }

                var orderCode = webhookData.GetProperty("orderCode").GetInt64().ToString();

                // Find payment by order code
                var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(orderCode);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for orderCode {OrderCode}", orderCode);
                    result.Success = false;
                    result.RedirectUrl = $"{GetFrontendUrl()}/payment-failed?reason=Payment not found";
                    result.Message = "Payment not found";
                    response.Data = result;
                    return response;
                }

                _logger.LogInformation("PayOS return successful for Payment {PaymentId}, OrderCode {OrderCode}",
                    payment.PaymentId, orderCode);

                result.PaymentId = payment.PaymentId;
                result.OrderCode = orderCode;

                // Auto-confirm payment if still pending
                if (payment.Status == PaymentStatus.Pending)
                {
                    _logger.LogInformation("Auto-confirming Payment {PaymentId} via Return URL", payment.PaymentId);

                    var confirmDto = new CompletePayment
                    {
                        PaymentId = payment.PaymentId,
                        ProductId = payment.ProductId,
                        ProductType = payment.ProductType,
                        Amount = payment.Amount,
                        PaymentMethod = PaymentGateway.PayOs.ToString()
                    };

                    var confirmResult = await ConfirmPaymentAsync(confirmDto, payment.UserId);

                    if (confirmResult.Success)
                    {
                        _logger.LogInformation("Payment {PaymentId} auto-confirmed successfully via Return URL", payment.PaymentId);
                        result.Success = true;
                        result.RedirectUrl = $"{GetFrontendUrl()}/payment-success?paymentId={payment.PaymentId}&orderCode={orderCode}";
                        result.Message = "Payment confirmed successfully";
                    }
                    else
                    {
                        // If confirm fails, still redirect to success (might already be processed by webhook)
                        _logger.LogWarning("Payment {PaymentId} confirmation failed or already processed: {Message}",
                            payment.PaymentId, confirmResult.Message);
                        result.Success = true;
                        result.RedirectUrl = $"{GetFrontendUrl()}/payment-success?paymentId={payment.PaymentId}&orderCode={orderCode}";
                        result.Message = "Payment already processed";
                    }
                }
                else
                {
                    _logger.LogInformation("Payment {PaymentId} already processed with status {Status}",
                        payment.PaymentId, payment.Status);
                    result.Success = true;
                    result.RedirectUrl = $"{GetFrontendUrl()}/payment-success?paymentId={payment.PaymentId}&orderCode={orderCode}";
                    result.Message = "Payment already processed";
                }

                response.Success = true;
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS return");
                result.Success = false;
                result.RedirectUrl = $"{GetFrontendUrl()}/payment-failed?reason=Server error";
                result.Message = "Server error";
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Error: {ex.Message}";
                response.Data = result;
                return response;
            }
        }

        private string GetFrontendUrl()
        {
            // This should be injected via IConfiguration, but for now using hardcoded
            // In real implementation, inject IConfiguration and get from appsettings
            return "http://localhost:3000";
        }
    }
}
