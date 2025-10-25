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
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _courseRepository = courseRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Processing payment for User {UserId}, Product {ProductId}, Type {TypeProduct}", userId, request.ProductId, request.typeproduct);

                decimal amount = 0m;
                var productType = request.typeproduct;

                // Tính amount dựa trên sản phẩm
                switch (productType)
                {
                    case TypeProduct.Course:
                        var course = await _courseRepository.GetCourseById(request.ProductId);
                        if (course == null)
                        {
                            _logger.LogWarning("Course {CourseId} not found", request.ProductId);
                            return new ServiceResponse<CreateInforPayment> { Success = false, Message = "Course not found" };
                        }
                        if (course.Price == null || course.Price <= 0)
                        {
                            _logger.LogWarning("Course {CourseId} has invalid price", request.ProductId);
                            return new ServiceResponse<CreateInforPayment> { Success = false, Message = "Course price is invalid" };
                        }
                        amount = course.Price.Value;
                        break;

                    case TypeProduct.TeacherPackage:
                        var package = await _teacherPackageRepository.GetTeacherPackageByIdAsync(request.ProductId);
                        if (package == null)
                        {
                            _logger.LogWarning("TeacherPackage {PackageId} not found", request.ProductId);
                            return new ServiceResponse<CreateInforPayment> { Success = false, Message = "TeacherPackage not found" };
                        }
                        if (package.Price <= 0)
                        {
                            _logger.LogWarning("TeacherPackage {PackageId} has invalid price", request.ProductId);
                            return new ServiceResponse<CreateInforPayment> { Success = false, Message = "TeacherPackage price is invalid" };
                        }
                        amount = package.Price;
                        break;

                    default:
                        _logger.LogWarning("Unsupported product type: {Type}", productType);
                        return new ServiceResponse<CreateInforPayment> { Success = false, Message = "Unsupported product type" };
                }

                // Tạo payment entity
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
                _logger.LogInformation("Payment {PaymentId} created successfully", payment.PaymentId);

                // Chuẩn bị response
                var createInforPayment = new CreateInforPayment
                {
                    PaymentId = payment.PaymentId,
                    ProductId = payment.ProductId,
                    ProductType = payment.ProductType,
                    Amount = payment.Amount
                };

                await _unitOfWork.CommitAsync();
                return new ServiceResponse<CreateInforPayment> { Success = true, Data = createInforPayment };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for User {UserId}", userId);
                await _unitOfWork.RollbackAsync();
                return new ServiceResponse<CreateInforPayment> { Success = false, Message = "An error occurred while processing payment" };
            }
        }

        public async Task<ServiceResponse<bool>> ConfirmPaymentAsync(CompletePayment paymentDto, int userId)
        {
            var existingPayment = await _paymentRepository.GetPaymentByIdAsync(paymentDto.PaymentId);
            if (existingPayment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentDto.PaymentId);
                return new ServiceResponse<bool> { Success = false, Message = "Payment not found" };
            }

            if (existingPayment.UserId != userId)
            {
                _logger.LogWarning("User {UserId} not authorized for Payment {PaymentId}", userId, paymentDto.PaymentId);
                return new ServiceResponse<bool> { Success = false, Message = "Unauthorized" };
            }

            if (existingPayment.Status != PaymentStatus.Pending)
            {
                _logger.LogWarning("Payment {PaymentId} already processed", paymentDto.PaymentId);
                return new ServiceResponse<bool> { Success = false, Message = "Payment already processed" };
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Confirming payment {PaymentId} for User {UserId}", paymentDto.PaymentId, userId);

                // Cập nhật payment
                existingPayment.PaymentMethod = paymentDto.PaymentMethod;
                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.PaidAt = DateTime.UtcNow;

                await _paymentRepository.UpdatePaymentStatusAsync(existingPayment);

                // Xử lý sau khi thanh toán thành công
                switch (existingPayment.ProductType)
                {
                    case TypeProduct.Course:
                        // Payment chỉ xác nhận thanh toán thành công
                        // User cần gọi POST /api/user/enroll/course để enroll (có đầy đủ validation)
                        // Điều này đảm bảo:
                        // - Kiểm tra course.CanJoin() (MaxStudent)
                        // - Kiểm tra Teacher Package limit (MaxStudents)
                        // - Logic enrollment tập trung tại UserEnrollmentService
                        _logger.LogInformation("Payment completed for Course {CourseId}. User {UserId} can now enroll via /api/user/enroll/course", 
                            existingPayment.ProductId, existingPayment.UserId);
                        break;

                    case TypeProduct.TeacherPackage:
                        var user = await _userRepository.GetByIdAsync(existingPayment.UserId);
                        if (user == null)
                        {
                            throw new KeyNotFoundException("User not found");
                        }
                        await _userRepository.UpdateRoleTeacher(existingPayment.UserId);
                        _logger.LogInformation("User {UserId} upgraded to Teacher role", existingPayment.UserId);
                        break;
                }

                await _paymentRepository.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Payment {PaymentId} confirmed successfully", paymentDto.PaymentId);
                return new ServiceResponse<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId} for User {UserId}", paymentDto.PaymentId, userId);
                await _unitOfWork.RollbackAsync();
                return new ServiceResponse<bool> { Success = false, Message = "An error occurred while confirming payment" };
            }
        }
    }
}
