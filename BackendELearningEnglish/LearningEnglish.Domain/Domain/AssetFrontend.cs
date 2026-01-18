using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Domain.Entities
{
    // Entity cho quản lý hình ảnh/tài nguyên frontend (banner, hero images, etc.)
    public class AssetFrontend
    {
        public int Id { get; set; }
        
        // Tên hiển thị của hình ảnh
        public string NameImage { get; set; } = string.Empty;
        
        // Key/path lưu trong MinIO
        public string KeyImage { get; set; } = string.Empty;
        
        // Loại asset: Logo, DefaultCourse, DefaultLesson
        public AssetType AssetType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}