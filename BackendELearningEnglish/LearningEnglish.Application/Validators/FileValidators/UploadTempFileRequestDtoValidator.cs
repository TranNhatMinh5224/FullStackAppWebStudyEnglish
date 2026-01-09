using FluentValidation;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Validators.FileValidators
{
    /// <summary>
    /// Validator for file upload requests
    /// Clean Architecture: Validation dựa trên content-type, KHÔNG dựa trên bucket names
    /// </summary>
    public class UploadTempFileRequestDtoValidator : AbstractValidator<UploadTempFileRequestDto>
    {
        private const long MAX_IMAGE_SIZE = 2_097_152;      // 2MB
        private const long MAX_AUDIO_SIZE = 5_242_880;      // 5MB
        private const long MAX_VIDEO_SIZE = 52_428_800;     // 50MB
        private const long MAX_DOCUMENT_SIZE = 10_485_760;  // 10MB

        public UploadTempFileRequestDtoValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required")
                .Must(file => file.Length > 0)
                .WithMessage("File cannot be empty")
                .Must((dto, file) => ValidateFileSize(file))
                .WithMessage((dto, file) => GetFileSizeErrorMessage(file));

            RuleFor(x => x.BucketName)
                .NotEmpty()
                .WithMessage("Bucket name is required");

            RuleFor(x => x.TempFolder)
                .MaximumLength(100)
                .WithMessage("Temp folder name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.TempFolder));
        }

        /// <summary>
        /// Validate file size based on content-type only (Clean Architecture compliant)
        /// </summary>
        private static bool ValidateFileSize(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return false;

            var contentType = file.ContentType?.ToLower() ?? "";
            long maxSize = GetMaxSizeByContentType(contentType);

            return file.Length <= maxSize;
        }

        /// <summary>
        /// Get max file size based on content-type
        /// </summary>
        private static long GetMaxSizeByContentType(string contentType)
        {
            if (contentType.StartsWith("image/"))
                return MAX_IMAGE_SIZE;
            
            if (contentType.StartsWith("audio/"))
                return MAX_AUDIO_SIZE;
            
            if (contentType.StartsWith("video/"))
                return MAX_VIDEO_SIZE;
            
            return MAX_DOCUMENT_SIZE;
        }

        private static string GetFileSizeErrorMessage(IFormFile? file)
        {
            if (file == null)
                return "File is required";

            var contentType = file.ContentType?.ToLower() ?? "";
            string fileType;
            string maxSizeReadable;

            if (contentType.StartsWith("image/"))
            {
                fileType = "Image";
                maxSizeReadable = "2MB";
            }
            else if (contentType.StartsWith("audio/"))
            {
                fileType = "Audio";
                maxSizeReadable = "5MB";
            }
            else if (contentType.StartsWith("video/"))
            {
                fileType = "Video";
                maxSizeReadable = "50MB";
            }
            else
            {
                fileType = "File";
                maxSizeReadable = "10MB";
            }

            var currentSizeMB = file.Length / 1024.0 / 1024.0;
            return $"{fileType} file size ({currentSizeMB:F2}MB) exceeds the maximum allowed size of {maxSizeReadable}.";
        }
    }
}

