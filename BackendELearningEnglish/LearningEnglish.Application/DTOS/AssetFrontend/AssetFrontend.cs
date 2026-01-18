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

        // URL công khai để truy cập ảnh
        public string? ImageUrl { get; set; }

        // Loại asset: Logo, DefaultCourse, DefaultLesson
        public AssetType AssetType { get; set; } = AssetType.Logo;

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateAssetFrontendDto
    {
        public string NameImage { get; set; } = string.Empty;
        public string? ImageTempKey { get; set; } // Key của file tạm thời
        public string? ImageType { get; set; } // Loại file (image/jpeg, image/png, etc.)
        public AssetType AssetType { get; set; } = AssetType.Logo;
    }

    public class UpdateAssetFrontendDto
    {
        public int Id { get; set; }
        public string? NameImage { get; set; }
        public string? ImageTempKey { get; set; } // Key của file tạm thời mới (nếu có)
        public string? ImageType { get; set; } // Loại file mới
        public AssetType? AssetType { get; set; }
    }
}