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
        private readonly IEmailService _emailService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository,
            IUserRepository userRepository,
            IUserEnrollmentService userEnrollmentService,
            IMapper mapper,
            ILogger<PaymentService> logger,
            IUnitOfWork unitOfWork,
            ITeacherSubscriptionService teacherSubscriptionService,
            IEmailService emailService)
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
            _emailService = emailService;
        }

        public async Task<ServiceResponse<CreateInforPayment>> ProcessPaymentAsync(int userId, requestPayment request)
        {
            var response = new ServiceResponse<CreateInforPayment>();
            try
            {
                _logger.LogInformation("Bắt đầu xử lý thanh toán cho User {UserId}, Sản phẩm {ProductId}, Loại {TypeProduct}",
                    userId, request.ProductId, request.typeproduct);

                // 1. Validate user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Xử lý thanh toán thất bại: Không tìm thấy User {UserId}", userId);
                    response.Success = false;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }
                // 2. Check if user already purchased this product
                var existingPayment = await _paymentRepository.GetSuccessfulPaymentByUserAndProductAsync(userId, request.ProductId, request.typeproduct);
                if (existingPayment != null)
                {
                    _logger.LogWarning("Xử lý thanh toán thất bại: User {UserId} đã mua {ProductType} {ProductId}",
                        userId, request.typeproduct, request.ProductId);
                    response.Success = false;
                    response.Message = "Bạn đã mua sản phẩm này rồi";
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
                            _logger.LogWarning("Xử lý thanh toán thất bại: Không tìm thấy khóa học {CourseId}", request.ProductId);
                            response.Success = false;
                            response.Message = "Không tìm thấy khóa học";
                            return response;
                        }
                        if (course.Price == null || course.Price <= 0)
                        {
                            _logger.LogWarning("Xử lý thanh toán thất bại: Khóa học {CourseId} có giá không hợp lệ {Price}",
                                request.ProductId, course.Price);
                            response.Success = false;
                            response.Message = "Giá khóa học không hợp lệ";
                            return response;
                        }
                        // Check if course can accept more students
                        if (!course.CanJoin())
                        {
                            _logger.LogWarning("Xử lý thanh toán thất bại: Khóa học {CourseId} đã đầy (Tối đa: {MaxStudent}, Hiện tại: {EnrollmentCount})",
                                request.ProductId, course.MaxStudent, course.EnrollmentCount);
                            response.Success = false;
                            response.Message = "Khóa học đã đầy";
                            return response;
                        }
                        amount = course.Price.Value;
                        _logger.LogInformation("Xác thực khóa học thành công: {CourseId}, Tiêu đề: {Title}, Giá: {Price}",
                            course.CourseId, course.Title, course.Price);
                        break;

                    case TypeProduct.TeacherPackage:
                        var package = await _teacherPackageRepository.GetTeacherPackageByIdAsync(request.ProductId);
                        if (package == null)
                        {
                            _logger.LogWarning("Xử lý thanh toán thất bại: Không tìm thấy gói giáo viên {PackageId}", request.ProductId);
                            response.Success = false;
                            response.Message = "Không tìm thấy gói giáo viên";
                            return response;
                        }
                        if (package.Price <= 0)
                        {
                            _logger.LogWarning("Xử lý thanh toán thất bại: Gói giáo viên {PackageId} có giá không hợp lệ {Price}",
                                request.ProductId, package.Price);
                            response.Success = false;
                            response.Message = "Giá gói giáo viên không hợp lệ";
                            return response;
                        }
                        amount = package.Price;
                        _logger.LogInformation("Xác thực gói giáo viên thành công: {PackageId}, Giá: {Price}",
                            package.TeacherPackageId, package.Price);
                        break;

                    default:
                        _logger.LogWarning("Xử lý thanh toán thất bại: Loại sản phẩm không được hỗ trợ: {Type}", productType);
                        response.Success = false;
                        response.Message = "Loại sản phẩm không được hỗ trợ";
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

                // Handle post-payment actions
                switch (existingPayment.ProductType)
                {
                    case TypeProduct.Course:
                        _logger.LogInformation("Thanh toán hoàn tất cho khóa học {CourseId}. Tự động đăng ký User {UserId}",
                            existingPayment.ProductId, existingPayment.UserId);

                        //  AUTO-ENROLL USER VÀO COURSE SAU KHI THANH TOÁN THÀNH CÔNG
                        var enrollDto = new EnrollCourseDto { CourseId = existingPayment.ProductId };
                        var enrollResult = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, existingPayment.UserId);

                        if (!enrollResult.Success)
                        {
                            _logger.LogWarning("Tự động đăng ký thất bại cho thanh toán {PaymentId}: {Message}",
                                paymentDto.PaymentId, enrollResult.Message);

                            _logger.LogError("Thanh toán {PaymentId} hoàn tất nhưng tự động đăng ký thất bại. User {UserId} có thể đăng ký thủ công vào khóa học {CourseId}",
                                paymentDto.PaymentId, existingPayment.UserId, existingPayment.ProductId);
                        }
                        else
                        {
                            _logger.LogInformation("User {UserId} đã được tự động đăng ký vào khóa học {CourseId} sau thanh toán {PaymentId}",
                                existingPayment.UserId, existingPayment.ProductId, paymentDto.PaymentId);

                            // Gửi email thông báo đã tham gia khóa học
                            try
                            {
                                var user = await _userRepository.GetByIdAsync(existingPayment.UserId);
                                var course = await _courseRepository.GetCourseById(existingPayment.ProductId);
                                if (user != null && course != null)
                                {
                                    await _emailService.SendNotifyJoinCourseAsync(user.Email, course.Title, user.FirstName);
                                    _logger.LogInformation("Email thông báo đã được gửi đến User {UserId} cho việc tham gia khóa học {CourseId}",
                                        existingPayment.UserId, existingPayment.ProductId);
                                }
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogWarning(emailEx, "Gửi email thông báo khóa học thất bại cho thanh toán {PaymentId}",
                                    paymentDto.PaymentId);
                            }
                        }
                        break;

                    case TypeProduct.TeacherPackage:
                        var user = await _userRepository.GetByIdAsync(existingPayment.UserId);
                        if (user == null)
                        {
                            response.Success = false;
                            response.Message = "Không tìm thấy người dùng";
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
                            _logger.LogError("Tạo đăng ký giáo viên thất bại cho thanh toán {PaymentId}: {Message}",
                                paymentDto.PaymentId, subscriptionResult.Message);
                            response.Success = false;
                            response.Message = "Tạo đăng ký giáo viên thất bại";
                            await _unitOfWork.RollbackAsync();
                            return response;
                        }

                        await _userRepository.SaveChangesAsync();
                        _logger.LogInformation("User {UserId} đã được nâng cấp lên vai trò giáo viên", existingPayment.UserId);

                        // Gửi email thông báo đã mua gói giáo viên
                        try
                        {
                            var nameteacherpackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(existingPayment.ProductId);
                            if (nameteacherpackage == null)
                            {
                                _logger.LogError("Không tìm thấy gói giáo viên cho thanh toán {PaymentId}", paymentDto.PaymentId);
                                response.Success = false;
                                response.Message = "Không tìm thấy gói giáo viên";
                                await _unitOfWork.RollbackAsync();
                                return response;
                            }

                            await _emailService.SendNotifyPurchaseTeacherPackageAsync(user.Email, nameteacherpackage.PackageName, user.FirstName, nameteacherpackage.Price, subscriptionResult.EndDate);
                            _logger.LogInformation("Email thông báo đã được gửi đến User {UserId} cho việc mua gói giáo viên {PackageId}",
                                existingPayment.UserId, existingPayment.ProductId);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Gửi email thông báo gói giáo viên thất bại cho thanh toán {PaymentId}",
                                paymentDto.PaymentId);
                        }
                        break;
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