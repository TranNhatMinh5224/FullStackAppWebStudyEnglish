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

    //  Lấy theo type 
    public async Task<List<AssetFrontend>> GetAssetByType(int type)
    {
        return await _context.AssetsFrontend
            .Where(a => a.AssetType == (AssetType)type)
            .ToListAsync();
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
        existingAsset.DescriptionImage = assetFrontend.DescriptionImage;
        existingAsset.AssetType = assetFrontend.AssetType;
        existingAsset.Order = assetFrontend.Order;
        existingAsset.IsActive = assetFrontend.IsActive;
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
}
