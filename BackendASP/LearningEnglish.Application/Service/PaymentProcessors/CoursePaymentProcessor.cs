using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.PaymentProcessors
{
    public class CoursePaymentProcessor : IPaymentStrategy
    {
        public ProductType ProductType => ProductType.Course;

        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly IPaymentNotificationService _notificationService;
        private readonly ILogger<CoursePaymentProcessor> _logger;

        public CoursePaymentProcessor(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IUserEnrollmentService userEnrollmentService,
            IPaymentNotificationService notificationService,
            ILogger<CoursePaymentProcessor> logger)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _userEnrollmentService = userEnrollmentService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ServiceResponse<decimal>> ValidateProductAsync(int productId)
        {
            var response = new ServiceResponse<decimal>();

            try
            {
                var course = await _courseRepository.GetCourseById(productId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                if (course.Price == null || course.Price <= 0)
                {
                    response.Success = false;
                    response.Message = "Giá khóa học không hợp lệ";
                    return response;
                }

                if (!course.CanJoin())
                {
                    response.Success = false;
                    response.Message = "Khóa học đã đầy";
                    return response;
                }

                response.Data = course.Price.Value;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate course {CourseId}", productId);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi kiểm tra khóa học";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                _logger.LogInformation("Thanh toán hoàn tất cho khóa học {CourseId}. Tự động đăng ký User {UserId}", productId, userId);

                // Auto-enroll user into course using post-payment method (skip payment check)
                var enrollDto = new EnrollCourseDto { CourseId = productId };
                var enrollResult = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, userId);

                if (!enrollResult.Success)
                {
                    _logger.LogWarning("Tự động đăng ký thất bại cho thanh toán {PaymentId}: {Message}", paymentId, enrollResult.Message);
                    _logger.LogError("Thanh toán {PaymentId} hoàn tất nhưng tự động đăng ký thất bại. User {UserId} có thể đăng ký thủ công vào khóa học {CourseId}",
                        paymentId, userId, productId);
                    
                    response.Success = false;
                    response.Message = "Thanh toán thành công nhưng đăng ký khóa học thất bại: " + enrollResult.Message;
                    return response;
                }

                _logger.LogInformation("User {UserId} đã được tự động đăng ký vào khóa học {CourseId} sau thanh toán {PaymentId}",
                    userId, productId, paymentId);

                // Send notification email
                try
                {
                    await _notificationService.SendCoursePaymentNotificationAsync(userId, productId);
                    _logger.LogInformation("Email thông báo đã được gửi đến User {UserId} cho việc tham gia khóa học {CourseId}",
                        userId, productId);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Gửi email thông báo khóa học thất bại cho thanh toán {PaymentId}", paymentId);
                    // Email failure should not affect payment success
                }

                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý post-payment cho course {CourseId}, User {UserId}", productId, userId);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi xử lý sau thanh toán";
                return response;
            }
        }
    }
}
