using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // DTO asset frontend
    public class AssetFrontendDto
    {
        public int Id { get; set; }

        // Tên hiển thị của hình ảnh
        public string NameImage { get; set; } = string.Empty;

        // Key/path lưu trong MinIO
        public string KeyImage { get; set; } = string.Empty;

        // Mô tả/alt text cho SEO
        public string? DescriptionImage { get; set; } = string.Empty;

        // Loại asset: "banner", "hero", "logo", "icon", etc.
        public AssetType AssetType { get; set; } = AssetType.Other;

        // Thứ tự hiển thị
        public int? Order { get; set; } = 0;

        // Bật/tắt hiển thị
        public bool? IsActive { get; set; } = true;

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class CreateAssetFrontendDto
    {
        public string NameImage { get; set; } = string.Empty;
        public string KeyImage { get; set; } = string.Empty;
        public string? DescriptionImage { get; set; } = string.Empty;
        public AssetType AssetType { get; set; } = AssetType.Other;
        public int? Order { get; set; } = 0;
        public bool? IsActive { get; set; } = true;
    }
    
    public class UpdateAssetFrontendDto
    {
        public int Id { get; set; }
        public string? NameImage { get; set; }
        public string? KeyImage { get; set; }
        public string? DescriptionImage { get; set; }
        public AssetType? AssetType { get; set; }
        public int? Order { get; set; }
        public bool? IsActive { get; set; }
    }
}