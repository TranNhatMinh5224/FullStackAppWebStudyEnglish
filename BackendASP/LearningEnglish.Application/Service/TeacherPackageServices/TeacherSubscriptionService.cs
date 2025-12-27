using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.TeacherPackage;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using AutoMapper;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using LearningEnglish.Application.Interface;

namespace LearningEnglish.Application.Service
{
    public class TeacherSubscriptionService : ITeacherSubscriptionService
    {
        private readonly ITeacherSubscriptionRepository _teacherSubscriptionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherSubscriptionService> _logger;

        public TeacherSubscriptionService(ITeacherSubscriptionRepository teacherSubscriptionRepository, IMapper mapper, ILogger<TeacherSubscriptionService> logger)
        {
            _teacherSubscriptionRepository = teacherSubscriptionRepository;
            _mapper = mapper;
            _logger = logger;
        }





        // xử lý mua gói teacher 




        public async Task<ServiceResponse<ResPurchaseTeacherPackageDto>> AddTeacherSubscriptionAsync(PurchaseTeacherPackageDto dto, int userId)
        { 
                  var response = new ServiceResponse<ResPurchaseTeacherPackageDto>();
            try
            {
                DateTime startDate;
                SubscriptionStatus status;

                // Check if user has an active subscription
                var existingSubscription = await _teacherSubscriptionRepository.GetActiveSubscriptionAsync(userId);

                if (existingSubscription != null && existingSubscription.EndDate > DateTime.UtcNow)
                {
                    // User has active subscription → Schedule new subscription to start after current ends
                    startDate = existingSubscription.EndDate.AddDays(1);
                    status = SubscriptionStatus.Pending; // Will auto-activate when StartDate arrives

                    _logger.LogInformation(
                        "User {UserId} has active subscription until {EndDate}. New subscription scheduled from {StartDate}",
                        userId, existingSubscription.EndDate, startDate);
                }
                else
                {
                    // No active subscription or expired → Start immediately
                    startDate = DateTime.UtcNow;
                    status = SubscriptionStatus.Active;

                    _logger.LogInformation("User {UserId} has no active subscription. New subscription starts immediately.", userId);
                }

                var teacherSubscription = new TeacherSubscription
                {
                    UserId = userId,
                    TeacherPackageId = dto.IdTeacherPackage,
                    StartDate = startDate,
                    EndDate = startDate.AddMonths(12),
                    Status = status
                };

                await _teacherSubscriptionRepository.AddTeacherSubscriptionAsync(teacherSubscription);

                var resultDto = _mapper.Map<ResPurchaseTeacherPackageDto>(teacherSubscription);

                string message = status == SubscriptionStatus.Pending
                    ? $"Teacher package purchased successfully. Will be activated on {startDate:yyyy-MM-dd}."
                    : "Teacher package purchased and activated successfully.";

                response.Data = resultDto;
                response.Success = true;
                response.StatusCode = 201;
                response.Message = message;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing teacher package.");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "An error occurred while purchasing the teacher package.";
                return response;

            }
        }
        // xử lý hủy gói teacher
        public async Task<ServiceResponse<bool>> DeleteTeacherSubscriptionAsync(DeleteTeacherSubscriptionDto dto, int userId)
        { 
            var response = new ServiceResponse<bool>();
            try
            {
                // Check ownership: user chỉ có thể xóa subscription của chính mình
                var teacherSubscription = await _teacherSubscriptionRepository.GetTeacherSubscriptionByIdAndUserIdAsync(
                    dto.TeacherSubscriptionId, userId);
                
                if (teacherSubscription == null)
                {
                    _logger.LogWarning("User {UserId} attempted to delete subscription {SubscriptionId} that doesn't exist or doesn't belong to them", 
                        userId, dto.TeacherSubscriptionId);
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Data = false;
                    response.Message = "Không tìm thấy subscription hoặc bạn không có quyền xóa subscription này";
                    return response;
                }

                await _teacherSubscriptionRepository.DeleteTeacherSubscriptionAsync(teacherSubscription);

                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Teacher subscription deleted successfully.";
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting teacher subscription {SubscriptionId} for user {UserId}", 
                    dto.TeacherSubscriptionId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Data = false;
                response.Message = "An error occurred while deleting the teacher subscription.";
                return response;
                
            }
        }
    }
}
