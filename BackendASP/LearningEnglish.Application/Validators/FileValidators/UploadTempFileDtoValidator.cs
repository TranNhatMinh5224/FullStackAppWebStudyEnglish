using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Validators.FileValidators
{
    public class UploadTempFileDtoValidator : AbstractValidator<IFormFile>
    {
        private const long MAX_IMAGE_SIZE = 2_097_152;      // 2MB
        private const long MAX_AUDIO_SIZE = 5_242_880;      // 5MB
        private const long MAX_VIDEO_SIZE = 52_428_800;     // 50MB
        private const long MAX_DOCUMENT_SIZE = 10_485_760;  // 10MB

        public UploadTempFileDtoValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("File is required")
                .Must(file => file.Length > 0)
                .WithMessage("File cannot be empty");

            RuleFor(x => x.Length)
                .Must((file, length) => ValidateFileSize(file, length))
                .WithMessage((file, length) => GetFileSizeErrorMessage(file, length));
        }

        private static bool ValidateFileSize(IFormFile file, long fileLength)
        {
            if (file == null || fileLength == 0)
                return false;

            var contentType = file.ContentType?.ToLower() ?? "";
            long maxSize;

            if (contentType.StartsWith("image/"))
            {
                maxSize = MAX_IMAGE_SIZE;
            }
            else if (contentType.StartsWith("audio/"))
            {
                maxSize = MAX_AUDIO_SIZE;
            }
            else if (contentType.StartsWith("video/"))
            {
                maxSize = MAX_VIDEO_SIZE;
            }
            else
            {
                maxSize = MAX_DOCUMENT_SIZE;
            }

            return fileLength <= maxSize;
        }

        private static string GetFileSizeErrorMessage(IFormFile file, long fileLength)
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
            else if (contentType.StartsWith("audio/"))
            {
                maxSize = MAX_AUDIO_SIZE;
                fileType = "Audio";
                maxSizeReadable = "5MB";
            }
            else if (contentType.StartsWith("video/"))
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

            if (fileLength > maxSize)
            {
                var currentSizeMB = fileLength / 1024.0 / 1024.0;
                return $"{fileType} file size ({currentSizeMB:F2}MB) exceeds the maximum allowed size of {maxSizeReadable}.";
            }

            return "File size is valid";
        }
    }
}

