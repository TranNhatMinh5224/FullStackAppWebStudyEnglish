using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Interface;
using LearningEnglish.Infrastructure.Data;
using LearningEnglish.Domain.Enums;
using Microsoft.EntityFrameworkCore;


public class AssetFrontendRepository : IAssetFrontendRepository
{
    private readonly AppDbContext _context;

    public AssetFrontendRepository(AppDbContext context)
    {
        _context = context;
    }

    //  Lấy tất cả
    public async Task<List<AssetFrontend>> GetAllAssetFrontend()
    {
        return await _context.AssetsFrontend.ToListAsync();
    }

    //  Lấy theo ID
    public async Task<AssetFrontend?> GetAssetFrontendById(int id)
    {
        return await _context.AssetsFrontend.FindAsync(id);
    }

    //  Lấy theo AssetType
    public async Task<AssetFrontend?> GetAssetByType(AssetType assetType)
    {
        return await _context.AssetsFrontend
            .FirstOrDefaultAsync(a => a.AssetType == assetType);
    }

    // Add
    public async Task<AssetFrontend> AddAssetFrontend(AssetFrontend assetFrontend)
    {
        await _context.AssetsFrontend.AddAsync(assetFrontend);
        await _context.SaveChangesAsync();
        return assetFrontend;
    }

    //  Update
    public async Task UpdateAssetFrontend(AssetFrontend assetFrontend)
    {
        var existingAsset = await _context.AssetsFrontend.FindAsync(assetFrontend.Id);
        if (existingAsset == null)
        {
            throw new ArgumentException("AssetFrontend không tồn tại");
        }

        existingAsset.NameImage = assetFrontend.NameImage;
        existingAsset.KeyImage = assetFrontend.KeyImage;
        existingAsset.AssetType = assetFrontend.AssetType;
        existingAsset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    //  Delete
    public async Task<AssetFrontend?> DeleteAssetFrontend(int id)
    {
        var assetFrontend = await _context.AssetsFrontend.FindAsync(id);
        if (assetFrontend == null)
        {
            return null;
        }

        _context.AssetsFrontend.Remove(assetFrontend);
        await _context.SaveChangesAsync();
        return assetFrontend;
    }

    // Public methods - lấy tất cả assets
    public async Task<List<AssetFrontend>> GetAllActiveAssetFrontend()
    {
        return await _context.AssetsFrontend.ToListAsync();
    }

}
