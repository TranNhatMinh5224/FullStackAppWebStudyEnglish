using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
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
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<CoursePaymentProcessor> _logger;

        public CoursePaymentProcessor(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IUserEnrollmentService userEnrollmentService,
            INotificationRepository notificationRepository,
            ILogger<CoursePaymentProcessor> logger)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _userEnrollmentService = userEnrollmentService;
            _notificationRepository = notificationRepository;
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

                if (course.Price == null || course.Price < 0)
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

        public async Task<string> GetProductNameAsync(int productId)
        {
            try
            {
                var course = await _courseRepository.GetCourseById(productId);
                return course?.Title ?? $"Khóa học #{productId}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tên course {CourseId}", productId);
                return $"Khóa học #{productId}";
            }
        }

        public async Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                _logger.LogInformation("Thanh toán hoàn tất cho khóa học {CourseId}. Tự động đăng ký User {UserId}", productId, userId);


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

                // Tạo notification thanh toán thành công
                try
                {
                    var course = await _courseRepository.GetByIdAsync(productId);
                    if (course != null)
                    {
                        var notification = new Notification
                        {
                            UserId = userId,
                            Title = "💳 Thanh toán thành công",
                            Message = $"Bạn đã thanh toán thành công khóa học '{course.Title}'. Chúc bạn học tốt!",
                            Type = NotificationType.PaymentSuccess,
                            RelatedEntityType = "Course",
                            RelatedEntityId = productId,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.AddAsync(notification);
                    }
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning(notifEx, "Tạo notification thất bại cho thanh toán {PaymentId}", paymentId);
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
