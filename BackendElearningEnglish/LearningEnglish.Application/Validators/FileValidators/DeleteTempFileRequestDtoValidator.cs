using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.FileValidators
{
    public class DeleteTempFileRequestDtoValidator : AbstractValidator<DeleteTempFileRequestDto>
    {
        public DeleteTempFileRequestDtoValidator()
        {
            RuleFor(x => x.BucketName)
                .NotEmpty()
                .WithMessage("bucketName is required");

            RuleFor(x => x.TempKey)
                .NotEmpty()
                .WithMessage("tempKey is required");
        }
    }
}

