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
using Microsoft.Extensions.Configuration;

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
        private readonly IConfiguration _configuration;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPaymentValidator paymentValidator,
            IEnumerable<IPaymentStrategy> paymentStrategies,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IUnitOfWork unitOfWork,
            IPayOSService payOSService,
            IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _paymentValidator = paymentValidator;
            _paymentStrategies = paymentStrategies;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _payOSService = payOSService;
            _configuration = configuration;
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
                    response.StatusCode = 400;
                    response.Message = userValidationResult.Message;
                    return response;
                }

                // 2 Validate product and get amount
                var productValidationResult = await _paymentValidator.ValidateProductAsync(request.ProductId, request.typeproduct);
                if (!productValidationResult.Success)
                {
                    response.Success = false;
                    response.StatusCode = 404;
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

                var baseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var random = new Random().Next(100, 999);
                var orderCode = baseTimestamp * 1000 + random;
                
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
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi xử lý thanh toán";
                return response;
            }
            response.Success = true;
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
                    _logger.LogWarning("Không tìm thấy thanh toán {PaymentId} cho User {UserId}", paymentDto.PaymentId, userId);
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy thanh toán";
                    return response;
                }

                // Explicit ownership check: user chỉ có thể confirm payment của chính mình
                if (existingPayment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} cố gắng confirm payment {PaymentId} của user khác (Owner: {OwnerId})", 
                        userId, paymentDto.PaymentId, existingPayment.UserId);
                    response.Success = false;
                    response.StatusCode = 403; // Forbidden
                    response.Message = "Bạn không có quyền xác nhận thanh toán này";
                    return response;
                }

                if (existingPayment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning("Thanh toán {PaymentId} đã được xử lý", paymentDto.PaymentId);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Thanh toán đã được xử lý";
                    return response;
                }

                // Validate Amount from DB - Security check to prevent client manipulation
                if (existingPayment.Amount != paymentDto.Amount)
                {
                    _logger.LogWarning("Amount mismatch cho Payment {PaymentId}. Expected: {ExpectedAmount}, Received: {ReceivedAmount}",
                        paymentDto.PaymentId, existingPayment.Amount, paymentDto.Amount);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Số tiền thanh toán không khớp";
                    return response;
                }

                // Validate ProductId and ProductType from DB
                if (existingPayment.ProductId != paymentDto.ProductId || existingPayment.ProductType != paymentDto.ProductType)
                {
                    _logger.LogWarning("Product mismatch cho Payment {PaymentId}. Expected: {ExpectedProduct}/{ExpectedType}, Received: {ReceivedProduct}/{ReceivedType}",
                        paymentDto.PaymentId, existingPayment.ProductId, existingPayment.ProductType, paymentDto.ProductId, paymentDto.ProductType);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Thông tin sản phẩm không khớp";
                    return response;
                }

                await _unitOfWork.BeginTransactionAsync();

                _logger.LogInformation("=== ConfirmPayment: PaymentId={PaymentId}, UserId={UserId} ===", paymentDto.PaymentId, userId);

                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.PaidAt = DateTime.UtcNow;
                existingPayment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdatePaymentStatusAsync(existingPayment);
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} status updated to Completed and saved", paymentDto.PaymentId);

                try
                {
                    var processor = _paymentStrategies.FirstOrDefault(s => s.ProductType == existingPayment.ProductType);
                    if (processor == null)
                    {
                        _logger.LogError("No payment strategy found for product type {ProductType}", existingPayment.ProductType);
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Loại sản phẩm không được hỗ trợ";
                        await _unitOfWork.RollbackAsync();
                        return response;
                    }

                    _logger.LogInformation("Calling ProcessPostPaymentAsync: UserId={UserId}, ProductId={ProductId}, ProductType={ProductType}", 
                        existingPayment.UserId, existingPayment.ProductId, existingPayment.ProductType);

                    var postPaymentResult = await processor.ProcessPostPaymentAsync(
                        existingPayment.UserId,
                        existingPayment.ProductId,
                        paymentDto.PaymentId);

                    _logger.LogInformation("ProcessPostPaymentAsync result: Success={Success}, Message={Message}", 
                        postPaymentResult.Success, postPaymentResult.Message);

                    if (!postPaymentResult.Success)
                    {
                        _logger.LogError("Post-payment processing failed for Payment {PaymentId}: {Message}",
                            paymentDto.PaymentId, postPaymentResult.Message);
                        response.Success = false;
                        response.StatusCode = 500;
                        response.Message = postPaymentResult.Message;
                        await _unitOfWork.RollbackAsync();
                        return response;
                    }
                }
                catch (NotSupportedException ex)
                {
                    _logger.LogError(ex, "Unsupported product type {ProductType} for Payment {PaymentId}",
                        existingPayment.ProductType, paymentDto.PaymentId);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Loại sản phẩm không được hỗ trợ";
                    await _unitOfWork.RollbackAsync();
                    return response;
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Xác nhận thanh toán {PaymentId} thành công", paymentDto.PaymentId);
                response.Success = true;
                response.StatusCode = 200; // Success
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId} for User {UserId}", paymentDto.PaymentId, userId);
                await _unitOfWork.RollbackAsync();
                response.Success = false;
                response.StatusCode = 500;
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
                response.Success = true;
                response.StatusCode = 200; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction history for User {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
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
                    _logger.LogWarning("User {UserId} cố gắng xem payment {PaymentId} không tồn tại hoặc không thuộc về mình", userId, paymentId);
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy giao dịch";
                    return response;
                }

                // Repository đã filter theo userId, nhưng thêm explicit check để đảm bảo
                if (payment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} cố gắng xem payment {PaymentId} của user khác (Owner: {OwnerId})", 
                        userId, paymentId, payment.UserId);
                    response.Success = false;
                    response.StatusCode = 403; // Forbidden
                    response.Message = "Bạn không có quyền xem giao dịch này";
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
                response.Success = true;
                response.StatusCode = 200; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction detail for Payment {PaymentId}", paymentId);
                response.Success = false;
                response.StatusCode = 500;
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

                var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for User {UserId}", paymentId, userId);
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy thanh toán";
                    return response;
                }

                // Explicit ownership check: user chỉ có thể tạo link cho payment của chính mình
                if (payment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} cố gắng tạo link cho payment {PaymentId} của user khác (Owner: {OwnerId})", 
                        userId, paymentId, payment.UserId);
                    response.Success = false;
                    response.StatusCode = 403; // Forbidden
                    response.Message = "Bạn không có quyền truy cập thanh toán này";
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
                
                // Description cho PayOS: dùng tên dịch vụ (PayOS giới hạn <= 9 ký tự)
                var description = !string.IsNullOrEmpty(payment.Description) 
                    ? payment.Description 
                    : productName;

                var linkRequest = new CreatePayOSLinkRequest
                {
                    PaymentId = payment.PaymentId
                };

                var linkResponse = await _payOSService.CreatePaymentLinkAsync(
                    linkRequest, 
                    payment.Amount, 
                    productName, 
                    description,
                    payment.OrderCode);

                // Xử lý code 231: OrderCode đã tồn tại → tạo orderCode mới
                if (!linkResponse.Success && linkResponse.Message?.Contains("231") == true)
                {
                    _logger.LogWarning("PayOS returned code 231 (orderCode exists) for Payment {PaymentId}. Generating new orderCode...", paymentId);
                    
                    // Tạo orderCode mới = PaymentId * 1000000000 + timestamp (đảm bảo unique)
                    var newOrderCode = (long)paymentId * 1000000000L + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    
                    // Update payment với orderCode mới
                    payment.OrderCode = newOrderCode;
                    payment.ProviderTransactionId = newOrderCode.ToString();
                    await _paymentRepository.UpdatePaymentStatusAsync(payment);
                    await _paymentRepository.SaveChangesAsync();
                    
                    _logger.LogInformation("Updated Payment {PaymentId} with new OrderCode: {OrderCode}", paymentId, newOrderCode);
                    
                    // Thử lại với orderCode mới
                    linkResponse = await _payOSService.CreatePaymentLinkAsync(
                        linkRequest, 
                        payment.Amount, 
                        productName, 
                        description,
                        newOrderCode);
                }

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
                response.StatusCode = 500;
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
                _logger.LogInformation("Processing webhook for OrderCode {OrderCode}", webhookData.OrderCode);

                var payment = await _paymentRepository.GetPaymentByTransactionIdAsync(webhookData.OrderCode.ToString());
                
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for OrderCode {OrderCode}", webhookData.OrderCode);
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Payment not found";
                    return response;
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    _logger.LogInformation("Payment {PaymentId} already completed", payment.PaymentId);
                    response.Success = true;
                    response.Data = true;
                    response.Message = "Payment already processed";
                    return response;
                }

                if (webhookData.Code != "00")
                {
                    _logger.LogWarning("Payment failed: Code={Code}, Desc={Desc}", webhookData.Code, webhookData.Desc);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Payment failed: {webhookData.Desc}";
                    return response;
                }

                string? paymentStatus = webhookData.Status;
                if (string.IsNullOrEmpty(paymentStatus) && !string.IsNullOrEmpty(webhookData.Data))
                {
                    try
                    {
                        var dataJson = JsonSerializer.Deserialize<JsonElement>(webhookData.Data);
                        if (dataJson.TryGetProperty("status", out var statusElement))
                        {
                            paymentStatus = statusElement.GetString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not parse status from webhook data");
                    }
                }

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
                    _logger.LogWarning("Payment {PaymentId} status is not PAID: Status={Status}", payment.PaymentId, paymentStatus ?? "null");
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Payment status is {paymentStatus ?? "unknown"}, not PAID";
                    return response;
                }

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

        // Xử lý PayOS webhook với signature verification (từ PayOS gọi trực tiếp)
        public async Task<ServiceResponse<bool>> ProcessPayOSWebhookAsync(PayOSWebhookDto webhookData)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                _logger.LogInformation("Processing PayOS webhook: code={Code}, orderCode={OrderCode}", 
                    webhookData.Code, webhookData.OrderCode);

                var isValid = await _payOSService.VerifyWebhookSignature(webhookData.Data, webhookData.Signature);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid webhook signature for OrderCode {OrderCode}", webhookData.OrderCode);
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Invalid signature";
                    return response;
                }

                // Process webhook using existing method
                return await ProcessWebhookFromQueueAsync(webhookData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook for OrderCode {OrderCode}", webhookData.OrderCode);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        // Xác nhận thanh toán PayOS (với validation PayOS status)
        public async Task<ServiceResponse<bool>> ConfirmPayOSPaymentAsync(int paymentId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Check payment ownership
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
                    _logger.LogWarning("User {UserId} attempted to confirm payment {PaymentId} of another user", userId, paymentId);
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Payment not found";
                    return response;
                }

                // Verify PayOS payment status if orderCode exists
                if (!string.IsNullOrEmpty(payment.ProviderTransactionId) &&
                    long.TryParse(payment.ProviderTransactionId, out var orderCode))
                {
                    var payosInfo = await _payOSService.GetPaymentInformationAsync(orderCode);
                    if (!payosInfo.Success || payosInfo.Data == null || payosInfo.Data.Code != "00")
                    {
                        _logger.LogWarning("Payment {PaymentId} not completed on PayOS. OrderCode: {OrderCode}", paymentId, orderCode);
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Payment not completed on PayOS";
                        return response;
                    }

                    if (string.IsNullOrEmpty(payosInfo.Data.Status) || !string.Equals(payosInfo.Data.Status, "PAID", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Payment {PaymentId} status is not PAID on PayOS. OrderCode: {OrderCode}, Status: {Status}", 
                            paymentId, orderCode, payosInfo.Data.Status ?? "null");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = $"Payment status is {payosInfo.Data.Status ?? "unknown"}, not PAID";
                        return response;
                    }
                }

                // Create CompletePayment DTO and confirm
                var confirmDto = new CompletePayment
                {
                    PaymentId = paymentId,
                    ProductId = payment.ProductId,
                    ProductType = payment.ProductType,
                    Amount = payment.Amount,
                    PaymentMethod = PaymentGateway.PayOs.ToString()
                };

                return await ConfirmPaymentAsync(confirmDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming PayOS payment {PaymentId} for User {UserId}", paymentId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        // Xử lý PayOS return URL
        public async Task<ServiceResponse<PayOSReturnResult>> ProcessPayOSReturnAsync(string code, string desc, string data, string? orderCode = null, string? status = null)
        {
            var response = new ServiceResponse<PayOSReturnResult>();
            var result = new PayOSReturnResult();

            try
            {
                _logger.LogInformation("Processing PayOS return: code={Code}, hasData={HasData}", code, !string.IsNullOrEmpty(data));

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

                Payment? payment = null;
                string? finalOrderCode = orderCode;

                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        var webhookData = JsonSerializer.Deserialize<JsonElement>(data);
                        if (webhookData.TryGetProperty("orderCode", out var orderCodeElement))
                        {
                            finalOrderCode = orderCodeElement.GetInt64().ToString();
                            payment = await _paymentRepository.GetPaymentByTransactionIdAsync(finalOrderCode);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error parsing PayOS return data");
                    }
                    catch (KeyNotFoundException ex)
                    {
                        _logger.LogError(ex, "PayOS return data missing orderCode");
                    }
                }

                if (payment == null && !string.IsNullOrEmpty(finalOrderCode) && long.TryParse(finalOrderCode, out var parsedOrderCode))
                {
                    payment = await _paymentRepository.GetPaymentByOrderCodeAsync(parsedOrderCode);
                }

                if (payment == null && !string.IsNullOrEmpty(data))
                {
                    try
                    {
                        var webhookData = JsonSerializer.Deserialize<JsonElement>(data);
                        if (webhookData.TryGetProperty("orderCode", out var orderCodeElement))
                        {
                            var orderCodeFromData = orderCodeElement.GetInt64();
                            payment = await _paymentRepository.GetPaymentByOrderCodeAsync(orderCodeFromData);
                            finalOrderCode = orderCodeFromData.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not parse orderCode from data");
                    }
                }

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found from PayOS return. Code={Code}, HasData={HasData}, OrderCode={OrderCode}", 
                        code, !string.IsNullOrEmpty(data), finalOrderCode ?? "null");
                    result.Success = false;
                    result.RedirectUrl = $"{GetFrontendUrl()}/payment-failed?reason=Payment not found";
                    result.Message = "Payment not found";
                    response.Data = result;
                    return response;
                }

                _logger.LogInformation("PayOS return successful for Payment {PaymentId}, OrderCode {OrderCode}",
                    payment.PaymentId, finalOrderCode ?? payment.OrderCode.ToString());

                result.PaymentId = payment.PaymentId;
                result.OrderCode = finalOrderCode ?? payment.OrderCode.ToString();

                string? paymentStatus = status;
                if (string.IsNullOrEmpty(paymentStatus) && !string.IsNullOrEmpty(finalOrderCode) && long.TryParse(finalOrderCode, out var orderCodeForStatus))
                {
                    var payosInfo = await _payOSService.GetPaymentInformationAsync(orderCodeForStatus);
                    if (payosInfo.Success && payosInfo.Data != null)
                    {
                        paymentStatus = payosInfo.Data.Status;
                    }
                }

                if (string.IsNullOrEmpty(paymentStatus) || !string.Equals(paymentStatus, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Payment {PaymentId} status is not PAID: Status={Status}", payment.PaymentId, paymentStatus ?? "null");
                    result.Success = false;
                    result.RedirectUrl = $"{GetFrontendUrl()}/payment-pending?orderCode={finalOrderCode}&status={Uri.EscapeDataString(paymentStatus ?? "")}";
                    result.Message = $"Payment status: {paymentStatus ?? "unknown"}";
                    response.Data = result;
                    return response;
                }

                // Auto-confirm payment if still pending and status is PAID
                if (payment.Status == PaymentStatus.Pending)
                {
                    _logger.LogInformation("Auto-confirming Payment {PaymentId} via Return URL (Status=PAID)", payment.PaymentId);

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
                    }
                    else
                    {
                        _logger.LogWarning("Payment {PaymentId} confirmation failed or already processed: {Message}",
                            payment.PaymentId, confirmResult.Message);
                    }
                }

                // Redirect thẳng về course page hoặc home
                if (payment.ProductType == ProductType.Course)
                {
                    result.Success = true;
                    result.RedirectUrl = $"{GetFrontendUrl()}/course/{payment.ProductId}";
                    result.Message = "Payment confirmed successfully";
                }
                else if (payment.ProductType == ProductType.TeacherPackage)
                {
                    result.Success = true;
                    result.RedirectUrl = $"{GetFrontendUrl()}/home";
                    result.Message = "Payment confirmed successfully";
                }
                else
                {
                    result.Success = true;
                    result.RedirectUrl = $"{GetFrontendUrl()}/home";
                    result.Message = "Payment confirmed successfully";
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
            return _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        }
    }
}
