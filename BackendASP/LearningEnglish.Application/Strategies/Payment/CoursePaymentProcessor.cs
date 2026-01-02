using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Strategies.Payment
{
    public class CoursePaymentProcessor : IPaymentStrategy
    {
        public ProductType ProductType => ProductType.Course;

        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CoursePaymentProcessor> _logger;

        public CoursePaymentProcessor(
            IUserEnrollmentService userEnrollmentService,
            INotificationRepository notificationRepository,
            ICourseRepository courseRepository,
            ILogger<CoursePaymentProcessor> logger)
        {
            _userEnrollmentService = userEnrollmentService;
            _notificationRepository = notificationRepository;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                _logger.LogInformation("=== Starting enrollment: PaymentId={PaymentId}, UserId={UserId}, CourseId={CourseId} ===", 
                    paymentId, userId, productId);

                var enrollDto = new EnrollCourseDto { CourseId = productId };
                var enrollResult = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, userId);

                _logger.LogInformation("=== Enrollment result: Success={Success}, StatusCode={StatusCode}, Message={Message} ===", 
                    enrollResult.Success, enrollResult.StatusCode, enrollResult.Message);

                if (!enrollResult.Success)
                {
                    _logger.LogError("Enrollment failed for Payment {PaymentId}: {Message}", paymentId, enrollResult.Message);
                    response.Success = false;
                    response.Message = "Thanh toán thành công nhưng đăng ký khóa học thất bại: " + enrollResult.Message;
                    return response;
                }

                _logger.LogInformation("=== Enrollment successful: User {UserId} enrolled in course {CourseId} after payment {PaymentId} ===",
                    userId, productId, paymentId);

                // Tạo notification thanh toán thành công
                try
                {
                    var course = await _courseRepository.GetCourseById(productId);
                    if (course != null)
                    {
                        var notification = new Notification
                        {
                            UserId = userId,
                            Title = "Thanh toán thành công",
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

        public async Task<ServiceResponse<decimal>> ValidateProductAsync(int productId)
        {
            var response = new ServiceResponse<decimal>();

            try
            {
                var course = await _courseRepository.GetCourseById(productId);
                if (course == null)
                {
                    _logger.LogWarning("Course {CourseId} không tồn tại", productId);
                    response.Success = false;
                    response.Message = "Khóa học không tồn tại";
                    return response;
                }
                response.Success = true;
                response.Data = course.Price ?? 0;
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
                return course?.Title ?? "Khóa học";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy tên course {CourseId}", productId);
                return "Khóa học";
            }
        }
    }
}
