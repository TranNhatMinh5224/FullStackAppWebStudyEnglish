using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAssetFrontendService
    {
        Task<ServiceResponse<List<AssetFrontendDto>>> GetAllAssetFrontends();
        Task<ServiceResponse<AssetFrontendDto>> GetAssetFrontendById(int id);
        Task<ServiceResponse<List<AssetFrontendDto>>> GetAssetsByTypeAsync(int assetType);
        Task<ServiceResponse<AssetFrontendDto>> AddAssetFrontend(CreateAssetFrontendDto newAssetFrontend);
        Task<ServiceResponse<bool>> UpdateAssetFrontend(UpdateAssetFrontendDto updatedAssetFrontend);
        Task<ServiceResponse<bool>> DeleteAssetFrontend(int id);
    }
}