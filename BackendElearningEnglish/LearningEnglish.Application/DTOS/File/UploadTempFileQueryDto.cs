namespace LearningEnglish.Application.DTOs
{
    public class UploadTempFileQueryDto
    {
        public string BucketName { get; set; } = string.Empty;
        public string TempFolder { get; set; } = "temp";
    }
}

