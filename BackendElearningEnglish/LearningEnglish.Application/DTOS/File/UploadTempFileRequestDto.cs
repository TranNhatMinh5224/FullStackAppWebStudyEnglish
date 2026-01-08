using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.DTOs
{
    public class UploadTempFileRequestDto
    {
        public IFormFile File { get; set; } = null!;
        public string BucketName { get; set; } = string.Empty;
        public string TempFolder { get; set; } = "temp";
    }
}

