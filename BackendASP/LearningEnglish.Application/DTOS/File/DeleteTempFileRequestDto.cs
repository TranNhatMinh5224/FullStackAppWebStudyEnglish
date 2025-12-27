namespace LearningEnglish.Application.DTOs
{
    public class DeleteTempFileRequestDto
    {
        public string BucketName { get; set; } = string.Empty;
        public string TempKey { get; set; } = string.Empty;
    }
}

