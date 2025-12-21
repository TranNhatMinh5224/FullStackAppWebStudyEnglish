namespace LearningEnglish.Domain.Entities
{
    /// <summary>
    /// Entity cho quản lý hình ảnh/tài nguyên frontend (banner, hero images, etc.)
    /// </summary>
    public class AssetFrontend
    {
        public int Id { get; set; }
        
        // Tên hiển thị của hình ảnh
        public string NameImage { get; set; } = string.Empty;
        
        // Key/path lưu trong MinIO
        public string KeyImage { get; set; } = string.Empty;
        
        // Mô tả/alt text cho SEO
        public string DescriptionImage { get; set; } = string.Empty;
        
        // Loại asset: "banner", "hero", "logo", "icon", etc.
        public string? AssetType { get; set; }
        
        // Thứ tự hiển thị
        public int Order { get; set; } = 0;
        
        // Bật/tắt hiển thị
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}