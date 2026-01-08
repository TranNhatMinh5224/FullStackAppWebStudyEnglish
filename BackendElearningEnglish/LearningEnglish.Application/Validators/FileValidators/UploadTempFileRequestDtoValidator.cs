using FluentValidation;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Validators.FileValidators
{
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
                .Must((dto, file) => ValidateFileSize(file, dto.BucketName))
                .WithMessage((dto, file) => GetFileSizeErrorMessage(file, dto.BucketName));

            RuleFor(x => x.BucketName)
                .NotEmpty()
                .WithMessage("Bucket name is required");

            RuleFor(x => x.TempFolder)
                .MaximumLength(100)
                .WithMessage("Temp folder name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.TempFolder));
        }

        private static bool ValidateFileSize(IFormFile? file, string bucketName)
        {
            if (file == null || file.Length == 0)
                return false;

            var contentType = file.ContentType?.ToLower() ?? "";
            long maxSize;

            // Xác định giới hạn kích thước dựa trên loại file hoặc bucketName
            if (contentType.StartsWith("image/"))
            {
                maxSize = MAX_IMAGE_SIZE;
            }
            else if (contentType.StartsWith("audio/") || bucketName == "flashcards")
            {
                maxSize = MAX_AUDIO_SIZE;
            }
            else if (contentType.StartsWith("video/") || bucketName == "lectures")
            {
                maxSize = MAX_VIDEO_SIZE;
            }
            else
            {
                maxSize = MAX_DOCUMENT_SIZE;
            }

            return file.Length <= maxSize;
        }

        private static string GetFileSizeErrorMessage(IFormFile? file, string bucketName)
        {
            if (file == null)
                return "File is required";

            var contentType = file.ContentType?.ToLower() ?? "";
            long maxSize;
            string fileType;
            string maxSizeReadable;

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
                return $"{fileType} file size ({currentSizeMB:F2}MB) exceeds the maximum allowed size of {maxSizeReadable}.";
            }

            return "File size is valid";
        }
    }
}

