using LearningEnglish.Infrastructure.MinioFileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/shared/files")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class FilesController : ControllerBase
    {
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly ILogger<FilesController> _logger;

        // File size limits (in bytes)
        private const long MAX_IMAGE_SIZE = 2_097_152;      // 2MB
        private const long MAX_AUDIO_SIZE = 5_242_880;      // 5MB
        private const long MAX_VIDEO_SIZE = 52_428_800;     // 50MB
        private const long MAX_DOCUMENT_SIZE = 10_485_760;  // 10MB

        public FilesController(IMinioFileStorage minioFileStorage, ILogger<FilesController> logger)
        {
            _minioFileStorage = minioFileStorage;
            _logger = logger;
        }

        // POST: api/shared/files/temp-file - tải lên file tạm thời
        [HttpPost("temp-file")]
        [RequestSizeLimit(52_428_800)]  // Max 50MB
        public async Task<IActionResult> UploadTemplateFile(IFormFile file, [FromQuery] string bucketName, [FromQuery] string tempFolder = "temp")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (string.IsNullOrEmpty(bucketName))
                return BadRequest("Bucket name is required.");

            var validationResult = ValidateFileSize(file, bucketName);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = validationResult.ErrorMessage,
                    maxSize = validationResult.MaxSizeReadable
                });
            }

            var result = await _minioFileStorage.UpLoadFileTempAsync(file, bucketName, tempFolder);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        private static (bool IsValid, string ErrorMessage, string MaxSizeReadable) ValidateFileSize(IFormFile file, string bucketName)
        {
            var contentType = file.ContentType.ToLower();
            long maxSize;
            string fileType;
            string maxSizeReadable;

            // xác định giới hạn kích thước dựa trên loại file
            if (contentType.StartsWith("image/"))
            {
                maxSize = MAX_IMAGE_SIZE;
                fileType = "Image";
                maxSizeReadable = "2MB";
            }
            else if (contentType.StartsWith("audio/") || bucketName == "flashcards")
            {
                maxSize = MAX_AUDIO_SIZE;
                fileType = "Audio";
                maxSizeReadable = "5MB";
            }
            else if (contentType.StartsWith("video/") || bucketName == "lectures")
            {
                maxSize = MAX_VIDEO_SIZE;
                fileType = "Video";
                maxSizeReadable = "50MB";
            }
            else
            {
                maxSize = MAX_DOCUMENT_SIZE;
                fileType = "File";
                maxSizeReadable = "10MB";
            }

            if (file.Length > maxSize)
            {
                var currentSizeMB = file.Length / 1024.0 / 1024.0;
                return (false,
                    $"{fileType} file size ({currentSizeMB:F2}MB) exceeds the maximum allowed size of {maxSizeReadable}.",
                    maxSizeReadable);
            }

            return (true, string.Empty, maxSizeReadable);
        }

        // DELETE: api/files/temp-file -xoá file tạm thời
        [HttpDelete("temp-file")]
        public async Task<IActionResult> DeleteTemp(
            [FromQuery] string bucketName,
            [FromQuery] string tempKey)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                return BadRequest("bucketName is required.");

            if (string.IsNullOrWhiteSpace(tempKey))
                return BadRequest("tempKey is required.");

            var result = await _minioFileStorage.DeleteFileAsync(tempKey, bucketName);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/shared/files/cleanup-temp - dọn dẹp file tạm thời (Admin only)
        [HttpPost("cleanup-temp")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupTempFiles(
            [FromServices] LearningEnglish.Application.Service.BackgroundJobs.TempFileCleanupJob cleanupJob)
        {
            try
            {
                _logger.LogInformation("Manual temp file cleanup triggered by admin");
                await cleanupJob.CleanupOldTempFilesAsync();
                return Ok(new
                {
                    success = true,
                    message = "Temp file cleanup completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual temp file cleanup");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error during cleanup",
                    error = ex.Message
                });
            }
        }
    }
}
