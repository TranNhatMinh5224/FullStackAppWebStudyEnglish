using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.FileValidators
{
    public class UploadTempFileQueryDtoValidator : AbstractValidator<UploadTempFileQueryDto>
    {
        public UploadTempFileQueryDtoValidator()
        {
            RuleFor(x => x.BucketName)
                .NotEmpty()
                .WithMessage("Bucket name is required");

            RuleFor(x => x.TempFolder)
                .MaximumLength(100)
                .WithMessage("Temp folder name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.TempFolder));
        }
    }
}

