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
                var teacherSubscription = new TeacherSubscription
                {
                    UserId = userId,
                    TeacherPackageId = dto.IdTeacherPackage,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    Status = SubscriptionStatus.Active
                };

                await _teacherSubscriptionRepository.AddTeacherSubscriptionAsync(teacherSubscription);

                var resultDto = _mapper.Map<ResPurchaseTeacherPackageDto>(teacherSubscription);
                return new ServiceResponse<ResPurchaseTeacherPackageDto>
                {
                    Data = resultDto,
                    Success = true,
                    Message = "Teacher package purchased successfully."
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
