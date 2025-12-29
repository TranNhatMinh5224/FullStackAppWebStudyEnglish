using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.TeacherPackage;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
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
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<TeacherPackagePaymentProcessor> _logger;

        public TeacherPackagePaymentProcessor(
            ITeacherPackageRepository teacherPackageRepository,
            IUserRepository userRepository,
            ITeacherSubscriptionService teacherSubscriptionService,
            INotificationRepository notificationRepository,
            IEmailService emailService,
            ILogger<TeacherPackagePaymentProcessor> logger)
        {
            _teacherPackageRepository = teacherPackageRepository;
            _userRepository = userRepository;
            _teacherSubscriptionService = teacherSubscriptionService;
            _notificationRepository = notificationRepository;
            _emailService = emailService;
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

                if (package.Price < 0)
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

        public async Task<string> GetProductNameAsync(int productId)
        {
            try
            {
                var package = await _teacherPackageRepository.GetTeacherPackageByIdAsync(productId);
                return package?.PackageName ?? $"Gói giáo viên #{productId}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tên teacher package {PackageId}", productId);
                return $"Gói giáo viên #{productId}";
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

                // 1. Add Teacher role to user (changes tracked by EF Core)
                var roleUpdated = await _userRepository.UpdateRoleTeacher(userId);
                if (!roleUpdated)
                {
                    _logger.LogError("Không thể cập nhật role Teacher cho User {UserId}", userId);
                    response.Success = false;
                    response.Message = "Không thể nâng cấp tài khoản lên giáo viên";
                    return response;
                }

                // 2. Create teacher subscription
                var subscription = new PurchaseTeacherPackageDto
                {
                    IdTeacherPackage = productId
                };

                var subscriptionResult = await _teacherSubscriptionService.AddTeacherSubscriptionAsync(subscription, userId);
                if (!subscriptionResult.Success)
                {
                    _logger.LogError("Tạo đăng ký giáo viên thất bại cho thanh toán {PaymentId}: {Message}",
                        paymentId, subscriptionResult.Message);
                    response.Success = false;
                    response.Message = "Tạo đăng ký giáo viên thất bại";
                    return response;
                }

                // 3. Save all changes (Role + Subscription) in one transaction
                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("User {UserId} đã được nâng cấp lên vai trò Teacher và kích hoạt subscription thành công", userId);

                // Tạo notification nâng cấp thành giáo viên
                try
                {
                    var teacherPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(productId);
                    if (teacherPackage != null)
                    {
                        var endDate = subscriptionResult.Data?.EndDate ?? DateTime.UtcNow.AddYears(1);
                        var notification = new Notification
                        {
                            UserId = userId,
                            Title = "Chúc mừng! Bạn đã trở thành giáo viên",
                            Message = $"Bạn đã đăng ký thành công gói '{teacherPackage.PackageName}'. Giá trị {teacherPackage.Price:N0} VNĐ, hết hạn {endDate:dd/MM/yyyy}.",
                            Type = NotificationType.PaymentSuccess,
                            RelatedEntityType = "TeacherPackage",
                            RelatedEntityId = productId,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _notificationRepository.AddAsync(notification);

                        // Gửi email xác nhận
                        try
                        {
                            await _emailService.SendNotifyPurchaseTeacherPackageAsync(
                                teacherUser.Email,
                                teacherPackage.PackageName,
                                teacherUser.FullName,
                                teacherPackage.Price,
                                endDate
                            );
                            _logger.LogInformation("Email xác nhận teacher package đã được gửi tới {Email}", teacherUser.Email);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Gửi email xác nhận teacher package thất bại cho User {UserId}", userId);
                        }
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
                _logger.LogError(ex, "Lỗi khi xử lý post-payment cho teacher package {PackageId}, User {UserId}", productId, userId);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi xử lý sau thanh toán";
                return response;
            }
        }
    }
}
