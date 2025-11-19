using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Common.Helpers
{
    /// <summary>
    /// Helper để xác định loại file và bucket tương ứng
    /// </summary>
    public static class FileTypeHelper
    {
        /// <summary>
        /// Xác định FileCategory từ ContentType và FileName
        /// </summary>
        public static FileCategory GetFileCategory(string contentType, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var mimeType = contentType.ToLowerInvariant();

            // Audio
            if (mimeType.StartsWith("audio/") || 
                new[] { ".mp3", ".wav", ".ogg", ".aac", ".m4a", ".flac", ".wma" }.Contains(extension))
            {
                return FileCategory.Audio;
            }

            // Image
            if (mimeType.StartsWith("image/") || 
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico" }.Contains(extension))
            {
                return FileCategory.Image;
            }

            // Video
            if (mimeType.StartsWith("video/") || 
                new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv", ".m4v" }.Contains(extension))
            {
                return FileCategory.Video;
            }

            // Document
            if (mimeType.Contains("pdf") || mimeType.Contains("document") || mimeType.Contains("text") ||
                new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf" }.Contains(extension))
            {
                return FileCategory.Document;
            }

            return FileCategory.Unknown;
        }

        /// <summary>
        /// Lấy tên bucket từ FileCategory
        /// </summary>
        public static string GetBucketName(FileCategory category)
        {
            return category switch
            {
                FileCategory.Audio => "Audio",
                FileCategory.Image => "Image",
                FileCategory.Video => "Video",
                FileCategory.Document => "Document",
                _ => "Image" // Default fallback
            };
        }

        /// <summary>
        /// Lấy folder path (temp hoặc real)
        /// </summary>
        public static string GetFolderPath(bool isTemp)
        {
            return isTemp ? "temp" : "real";
        }

        /// <summary>
        /// Extract FileCategory từ key (key có format: "image/temp/..." hoặc "audio/real/...")
        /// </summary>
        public static FileCategory ExtractCategoryFromKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return FileCategory.Unknown;

            var lowerKey = key.ToLowerInvariant();
            
            if (lowerKey.StartsWith("audio/"))
                return FileCategory.Audio;
            if (lowerKey.StartsWith("image/"))
                return FileCategory.Image;
            if (lowerKey.StartsWith("video/"))
                return FileCategory.Video;
            if (lowerKey.StartsWith("document/"))
                return FileCategory.Document;

            return FileCategory.Unknown;
        }
    }
}
