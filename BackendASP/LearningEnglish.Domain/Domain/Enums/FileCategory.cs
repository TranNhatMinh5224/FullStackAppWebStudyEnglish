namespace LearningEnglish.Domain.Enums
{
    /// <summary>
    /// Loại file để xác định bucket trong MinIO
    /// </summary>
    public enum FileCategory
    {
        Audio = 1,      // → Bucket: "Audio"
        Image = 2,      // → Bucket: "Image"
        Video = 3,      // → Bucket: "Video"
        Document = 4,   // → Bucket: "Document"
        Unknown = 0     // → Không xác định được
    }
}

