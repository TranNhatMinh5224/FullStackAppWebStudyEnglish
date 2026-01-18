using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;


namespace LearningEnglish.Application.Interface.AdminManagement
{
    public interface IAssetFrontendService
    {
        // Admin methods
        Task<ServiceResponse<List<AssetFrontendDto>>> GetAllAssetFrontends();
        Task<ServiceResponse<AssetFrontendDto>> AddAssetFrontend(CreateAssetFrontendDto newAssetFrontend);
        Task<ServiceResponse<bool>> UpdateAssetFrontend(UpdateAssetFrontendDto updatedAssetFrontend);
        Task<ServiceResponse<bool>> DeleteAssetFrontend(int id);
        
        // Public method - chỉ lấy assets đang active (dùng cho frontend)
        Task<ServiceResponse<List<AssetFrontendDto>>> GetAllActiveAssetFrontends();
    }
}