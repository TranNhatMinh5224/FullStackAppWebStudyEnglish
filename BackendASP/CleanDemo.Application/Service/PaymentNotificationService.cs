using CleanDemo.Application.Interface;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class PaymentNotificationService : IPaymentNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly ILogger<PaymentNotificationService> _logger;

        public PaymentNotificationService(
            IEmailService emailService,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository,
            ILogger<PaymentNotificationService> logger)
        {
            _emailService = emailService;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _logger = logger;
        }

        public async Task SendCoursePaymentNotificationAsync(int userId, int courseId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                var course = await _courseRepository.GetCourseById(courseId);

                if (user != null && course != null)
                {
                    await _emailService.SendNotifyJoinCourseAsync(user.Email, course.Title, user.FirstName);
                    _logger.LogInformation("Course payment notification sent to {Email} for Course {CourseId}",
                        user.Email, course.CourseId);
                }
                else
                {
                    _logger.LogWarning("Cannot send course notification - User or Course not found for UserId {UserId}, CourseId {CourseId}",
                        userId, courseId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send course payment notification for UserId {UserId}, CourseId {CourseId}",
                    userId, courseId);
                throw;
            }
        }

        public async Task SendTeacherPackagePaymentNotificationAsync(int userId, int packageId, DateTime validUntil)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                var teacherPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(packageId);

                if (user != null && teacherPackage != null)
                {
                    await _emailService.SendNotifyPurchaseTeacherPackageAsync(
                        user.Email, 
                        teacherPackage.PackageName, 
                        user.FirstName, 
                        teacherPackage.Price, 
                        validUntil);

                    _logger.LogInformation("Teacher package payment notification sent to {Email} for Package {PackageId}",
                        user.Email, teacherPackage.TeacherPackageId);
                }
                else
                {
                    _logger.LogWarning("Cannot send teacher package notification - User or Package not found for UserId {UserId}, PackageId {PackageId}",
                        userId, packageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send teacher package payment notification for UserId {UserId}, PackageId {PackageId}",
                    userId, packageId);
                throw;
            }
        }
    }
}
