
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
namespace LearningEnglish.Application.Interface.Services.TeacherPackage
{
    public interface ITeacherSubscriptionService
    {
        // Mua gói giáo viên
        Task<ServiceResponse<ResPurchaseTeacherPackageDto>> AddTeacherSubscriptionAsync(PurchaseTeacherPackageDto dto, int userId);
        
        // Hủy gói giáo viên
        Task<ServiceResponse<bool>> DeleteTeacherSubscriptionAsync(DeleteTeacherSubscriptionDto dto);
    }
}
