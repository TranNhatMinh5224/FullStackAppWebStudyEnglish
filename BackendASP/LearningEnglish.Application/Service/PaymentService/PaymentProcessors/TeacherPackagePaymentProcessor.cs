using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.PaymentProcessors
{
    public class TeacherPackagePaymentProcessor : IPaymentStrategy
    {
        public ProductType ProductType => ProductType.TeacherPackage;

        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITeacherSubscriptionService _teacherSubscriptionService;
        private readonly IPaymentNotificationService _notificationService;
        private readonly ILogger<TeacherPackagePaymentProcessor> _logger;

        public TeacherPackagePaymentProcessor(
            ITeacherPackageRepository teacherPackageRepository,
            IUserRepository userRepository,
            ITeacherSubscriptionService teacherSubscriptionService,
            IPaymentNotificationService notificationService,
            ILogger<TeacherPackagePaymentProcessor> logger)
        {
            _teacherPackageRepository = teacherPackageRepository;
            _userRepository = userRepository;
            _teacherSubscriptionService = teacherSubscriptionService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ServiceResponse<decimal>> ValidateProductAsync(int productId)
        {
            var response = new ServiceResponse<decimal>();

            try
            {
                var package = await _teacherPackageRepository.GetTeacherPackageByIdAsync(productId);
                if (package == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy gói giáo viên";
                    return response;
                }

                if (package.Price <= 0)
                {
                    response.Success = false;
                    response.Message = "Giá gói giáo viên không hợp lệ";
                    return response;
                }

                response.Data = package.Price;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate teacher package {PackageId}", productId);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi kiểm tra gói giáo viên";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> ProcessPostPaymentAsync(int userId, int productId, int paymentId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var teacherUser = await _userRepository.GetByIdAsync(userId);
                if (teacherUser == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                var subscription = new PurchaseTeacherPackageDto
                {
                    IdTeacherPackage = productId
                };

                await _userRepository.UpdateRoleTeacher(userId);

                var subscriptionResult = await _teacherSubscriptionService.AddTeacherSubscriptionAsync(subscription, userId);
                if (!subscriptionResult.Success)
                {
                    _logger.LogError("Tạo đăng ký giáo viên thất bại cho thanh toán {PaymentId}: {Message}",
                        paymentId, subscriptionResult.Message);
                    response.Success = false;
                    response.Message = "Tạo đăng ký giáo viên thất bại";
                    return response;
                }

                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("User {UserId} đã được nâng cấp lên vai trò giáo viên", userId);

                // Get teacher package details and send notification
                try
                {
                    var teacherPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(productId);
                    if (teacherPackage == null)
                    {
                        _logger.LogError("Không tìm thấy gói giáo viên cho thanh toán {PaymentId}", paymentId);
                        response.Success = false;
                        response.Message = "Không tìm thấy gói giáo viên";
                        return response;
                    }

                    var endDate = subscriptionResult.Data?.EndDate ?? DateTime.Now.AddYears(1);
                    await _notificationService.SendTeacherPackagePaymentNotificationAsync(userId, productId, endDate);

                    _logger.LogInformation("Email thông báo đã được gửi đến User {UserId} cho việc mua gói giáo viên {PackageId}",
                        userId, productId);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Gửi email thông báo gói giáo viên thất bại cho thanh toán {PaymentId}", paymentId);
                    // Email failure should not affect payment success
                }

                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý post-payment cho teacher package {PackageId}, User {UserId}", productId, userId);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi xử lý sau thanh toán";
                return response;
            }
        }
    }
}
