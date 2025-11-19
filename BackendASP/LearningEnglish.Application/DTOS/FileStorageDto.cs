namespace LearningEnglish.Application.DTOs
{
    // DTO Response khi upload file tạm thành công
    public class UploadTempFileResponseDto
    {
        public string TempKey { get; set; } = string.Empty; // Key dùng để tham chiếu file tạm
        public string PreviewUrl { get; set; } = string.Empty; // URL dùng để xem trước file tạm
        public string FileName { get; set; } = string.Empty; // Tên file gốc
        public long FileSize { get; set; } // Kích thước file
        public string ContentType { get; set; } = string.Empty; // Loại nội dung (MIME type)
    }

    // DTO Request khi chuyển file tạm thành file thật
    public class ConvertTempToRealFileRequestDto
    {
        public string TempKey { get; set; } = string.Empty;
        public string RealFolderPath { get; set; } = string.Empty; // Ví dụ: "courses", "lessons", "profile", "flashcards"
    }

    // DTO Response khi chuyển file tạm thành file thật thành công
    public class ConvertTempToRealFileResponseDto
    {
        public string RealKey { get; set; } = string.Empty;
        public string RealUrl { get; set; } = string.Empty;
    }

    // DTO Request khi xóa file tạm
    public class DeleteTempFileRequestDto
    {
        public string TempKey { get; set; } = string.Empty;
    }

    // DTO Request khi xóa file thật
    public class DeleteRealFileRequestDto
    {
        public string FileKey { get; set; } = string.Empty;
    }
}