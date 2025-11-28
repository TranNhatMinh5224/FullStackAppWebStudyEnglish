using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using AutoMapper;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;


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

                return new ServiceResponse<ResPurchaseTeacherPackageDto>
                {
                    Data = resultDto,
                    Success = true,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing teacher package.");
                return new ServiceResponse<ResPurchaseTeacherPackageDto>
                {
                    Data = null,
                    Success = false,
                    Message = "An error occurred while purchasing the teacher package."
                };
            }
        }
        // xử lý hủy gói teacher
        public async Task<ServiceResponse<bool>> DeleteTeacherSubscriptionAsync(DeleteTeacherSubscriptionDto dto)
        {
            try
            {
                var teacherSubscription = new TeacherSubscription
                {
                    TeacherSubscriptionId = dto.TeacherSubscriptionId
                };

                await _teacherSubscriptionRepository.DeleteTeacherSubscriptionAsync(teacherSubscription);

                return new ServiceResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = "Teacher subscription deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting teacher subscription.");
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Message = "An error occurred while deleting the teacher subscription."
                };
            }
        }
    }
}
