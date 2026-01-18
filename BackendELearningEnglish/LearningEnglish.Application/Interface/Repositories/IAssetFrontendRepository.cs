using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IAssetFrontendRepository
    {
        // Admin methods
        Task<List<AssetFrontend>> GetAllAssetFrontend();
        Task<AssetFrontend?> GetAssetFrontendById(int id);
        Task<AssetFrontend?> GetAssetByType(AssetType assetType);
        Task<AssetFrontend> AddAssetFrontend(AssetFrontend assetFrontend);
        Task UpdateAssetFrontend(AssetFrontend assetFrontend);
        Task<AssetFrontend?> DeleteAssetFrontend(int id);
        
        // Public method - chỉ lấy assets đang active (dùng cho frontend)
        Task<List<AssetFrontend>> GetAllActiveAssetFrontend();
    }
}