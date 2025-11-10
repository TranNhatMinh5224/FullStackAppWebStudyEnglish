// Sử dụng để lưu Minio + NginX 
namespace CleanDemo.Domain.Entities
{
    public class MediaAsset
    {
        public int MediaAssetId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        
        // Navigation Properties
        public List<QuizGroup> QuizGroups { get; set; } = new();
    }
}