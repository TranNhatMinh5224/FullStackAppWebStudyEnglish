using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IAssetFrontendRepository
    {
        Task<List<AssetFrontend>> GetAllAssetFrontend();
        Task<AssetFrontend?> GetAssetFrontendById(int id);
        Task<List<AssetFrontend>> GetAssetByType(int type);
        Task<AssetFrontend> AddAssetFrontend(AssetFrontend assetFrontend);
        Task UpdateAssetFrontend(AssetFrontend assetFrontend);
        Task<AssetFrontend?> DeleteAssetFrontend(int id);
    }
}