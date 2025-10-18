
using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
namespace CleanDemo.Application.Interface
{
    public interface ITeacherSubscriptionService
    {
        Task<ServiceResponse<ResPurchaseTeacherPackageDto>> AddTeacherSubscriptionAsync(PurchaseTeacherPackageDto dto, int userId);
        Task<ServiceResponse<bool>> DeleteTeacherSubscriptionAsync(DeleteTeacherSubscriptionDto dto);
    }
}
