using AutoMapper;
using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Domain.Enums;
using Microsoft.Extensions.Logging;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Service
{
    // Dịch vụ thanh toán hoàn chỉnh (bỏ validation cơ bản)
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeacherSubscriptionService _teacherSubscriptionService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository,
            IUserRepository userRepository,
            IUserEnrollmentService userEnrollmentService,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IUnitOfWork unitOfWork,
            ITeacherSubscriptionService teacherSubscriptionService)
        {
            _paymentRepository = paymentRepository;
            _courseRepository = courseRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _userRepository = userRepository;
            _userEnrollmentService = userEnrollmentService;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _teacherSubscriptionService = teacherSubscriptionService;
        }

        public async Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment request)
        {
            var response = new ServiceResponse<CreateInforPayment>();
            try
            {
                _logger.LogInformation("Starting payment process for User {UserId}, Product {ProductId}, Type {TypeProduct}",
                    userId, request.ProductId, request.typeproduct);

                // 1. Validate user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Payment process failed: User {UserId} not found", userId);
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                // 2. Check if user already purchased this product
                var existingPayment = await _paymentRepository.GetSuccessfulPaymentByUserAndProductAsync(userId, request.ProductId, request.typeproduct);
                if (existingPayment != null)
                {
                    _logger.LogWarning("Payment process failed: User {UserId} already purchased {ProductType} {ProductId}",
                        userId, request.typeproduct, request.ProductId);
                    response.Success = false;
                    response.Message = "You have already purchased this product";
                    return response;
                }

                decimal amount = 0m;
                var productType = request.typeproduct;

                // 3. Validate product and calculate amount
                switch (productType)
                {
                    case TypeProduct.Course:
                        var course = await _courseRepository.GetCourseById(request.ProductId);
                        if (course == null)
                        {
                            _logger.LogWarning("Payment process failed: Course {CourseId} not found", request.ProductId);
                            response.Success = false;
                            response.Message = "Course not found";
                            return response;
                        }
                        if (course.Price == null || course.Price <= 0)
                        {
                            _logger.LogWarning("Payment process failed: Course {CourseId} has invalid price {Price}",
                                request.ProductId, course.Price);
                            response.Success = false;
                            response.Message = "Course price is invalid";
                            return response;
                        }
                        // Check if course can accept more students
                        if (!course.CanJoin())
                        {
                            _logger.LogWarning("Payment process failed: Course {CourseId} is full (Max: {MaxStudent}, Current: {EnrollmentCount})", request.ProductId, course.MaxStudent, course.EnrollmentCount);
                            response.Success = false;
                            response.Message = "Course is full";
                            return response;
                        }
                        amount = course.Price.Value;
                        _logger.LogInformation("Course validation passed: {CourseId}, Title: {Title}, Price: {Price}",
                            course.CourseId, course.Title, course.Price);
                        break;

                    case TypeProduct.TeacherPackage:
                        var package = await _teacherPackageRepository.GetTeacherPackageByIdAsync(request.ProductId);
                        if (package == null)
                        {
                            _logger.LogWarning("Payment process failed: TeacherPackage {PackageId} not found", request.ProductId);
                            response.Success = false;
                            response.Message = "TeacherPackage not found";
                            return response;
                        }
                        if (package.Price <= 0)
                        {
                            _logger.LogWarning("Payment process failed: TeacherPackage {PackageId} has invalid price {Price}",
                                request.ProductId, package.Price);
                            response.Success = false;
                            response.Message = "TeacherPackage price is invalid";
                            return response;
                        }
                        amount = package.Price;
                        _logger.LogInformation("TeacherPackage validation passed: {PackageId}, Price: {Price}",
                            package.TeacherPackageId, package.Price);
                        break;

                    default:
                        _logger.LogWarning("Payment process failed: Unsupported product type: {Type}", productType);
                        response.Success = false;
                        response.Message = "Unsupported product type";
                        return response;
                }

                // 4. Create payment within transaction
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var payment = new Payment
                    {
                        UserId = userId,
                        ProductType = productType,
                        ProductId = request.ProductId,
                        Amount = amount,
                        Status = PaymentStatus.Pending,
                        PaidAt = null,
                        ProviderTransactionId = null
                    };

                    await _paymentRepository.AddPaymentAsync(payment);
                    await _paymentRepository.SaveChangesAsync();

                    _logger.LogInformation("Payment {PaymentId} created successfully for User {UserId}, Amount: {Amount}",
                        payment.PaymentId, userId, amount);


                    response.Data = new CreateInforPayment
                    {
                        PaymentId = payment.PaymentId,
                        ProductId = payment.ProductId,
                        ProductType = payment.ProductType,
                        Amount = payment.Amount
                    };

                    await _unitOfWork.CommitAsync();
                    _logger.LogInformation("Payment process completed successfully for Payment {PaymentId}", payment.PaymentId);
                }
                catch (Exception transactionEx)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger.LogError(transactionEx, "Transaction failed during payment creation for User {UserId}", userId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment process failed for User {UserId}, Product {ProductId}, Type {TypeProduct}",
                    userId, request.ProductId, request.typeproduct);
                response.Success = false;
                response.Message = "An error occurred while processing payment";
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
                    _logger.LogWarning("Payment {PaymentId} not found", paymentDto.PaymentId);
                    response.Success = false;
                    response.Message = "Payment not found";
                    return response;
                }

                if (existingPayment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} not authorized for Payment {PaymentId}", userId, paymentDto.PaymentId);
                    response.Success = false;
                    response.Message = "Unauthorized";
                    return response;
                }

                if (existingPayment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning("Payment {PaymentId} already processed", paymentDto.PaymentId);
                    response.Success = false;
                    response.Message = "Payment already processed";
                    return response;
                }

                await _unitOfWork.BeginTransactionAsync();

                _logger.LogInformation("Confirming payment {PaymentId} for User {UserId}", paymentDto.PaymentId, userId);

                // Update payment status
                existingPayment.PaymentMethod = paymentDto.PaymentMethod;
                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.PaidAt = DateTime.UtcNow;

                await _paymentRepository.UpdatePaymentStatusAsync(existingPayment);

                // Handle post-payment actions
                switch (existingPayment.ProductType)
                {
                    case TypeProduct.Course:
                        _logger.LogInformation("Payment completed for Course {CourseId}. Auto-enrolling User {UserId}",
                            existingPayment.ProductId, existingPayment.UserId);

                        //  AUTO-ENROLL USER VÀO COURSE SAU KHI THANH TOÁN THÀNH CÔNG
                        var enrollDto = new EnrollCourseDto { CourseId = existingPayment.ProductId };
                        var enrollResult = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, existingPayment.UserId);

                        if (!enrollResult.Success)
                        {
                            _logger.LogWarning("Auto-enrollment failed for Payment {PaymentId}: {Message}",
                                paymentDto.PaymentId, enrollResult.Message);



                            _logger.LogError("Payment {PaymentId} completed but auto-enrollment failed. User {UserId} can manually enroll in Course {CourseId}",
                                paymentDto.PaymentId, existingPayment.UserId, existingPayment.ProductId);
                        }
                        else
                        {
                            _logger.LogInformation("User {UserId} successfully auto-enrolled in Course {CourseId} after payment {PaymentId}",
                                existingPayment.UserId, existingPayment.ProductId, paymentDto.PaymentId);
                        }
                        break;
                    case TypeProduct.TeacherPackage:
                        var user = await _userRepository.GetByIdAsync(existingPayment.UserId);
                        if (user == null)
                        {
                            response.Success = false;
                            response.Message = "User not found";
                            await _unitOfWork.RollbackAsync();
                            return response;
                        }
                        var subscription = new PurchaseTeacherPackageDto
                        {
                            IdTeacherPackage = existingPayment.ProductId
                        };
                        await _userRepository.UpdateRoleTeacher(existingPayment.UserId);
                        var subscriptionResult = await _teacherSubscriptionService.AddTeacherSubscriptionAsync(subscription, existingPayment.UserId);
                        if (!subscriptionResult.Success)
                        {
                            _logger.LogError("Failed to create teacher subscription for Payment {PaymentId}: {Message}",
                                paymentDto.PaymentId, subscriptionResult.Message);
                            response.Success = false;
                            response.Message = "Failed to create teacher subscription";
                            await _unitOfWork.RollbackAsync();
                            return response;
                        }
                        await _userRepository.SaveChangesAsync();
                        _logger.LogInformation("User {UserId} upgraded to Teacher role", existingPayment.UserId);
                        break;
                }

                await _paymentRepository.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Payment {PaymentId} confirmed successfully", paymentDto.PaymentId);
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId} for User {UserId}", paymentDto.PaymentId, userId);
                await _unitOfWork.RollbackAsync();
                response.Success = false;
                response.Message = "An error occurred while confirming payment";
                response.Data = false;
                return response;
            }
            return response;
        }
    }
}
