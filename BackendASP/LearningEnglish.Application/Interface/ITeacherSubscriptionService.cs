
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
namespace LearningEnglish.Application.Interface
{
    public interface ITeacherSubscriptionService
    {
        Task<ServiceResponse<ResPurchaseTeacherPackageDto>> AddTeacherSubscriptionAsync(PurchaseTeacherPackageDto dto, int userId);
        Task<ServiceResponse<bool>> DeleteTeacherSubscriptionAsync(DeleteTeacherSubscriptionDto dto);
    }
}
