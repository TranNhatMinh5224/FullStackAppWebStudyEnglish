using FluentValidation;

namespace LearningEnglish.Application.Validators.FileValidators
{
    public class DeleteTempFileDtoValidator : AbstractValidator<(string bucketName, string tempKey)>
    {
        public DeleteTempFileDtoValidator()
        {
            RuleFor(x => x.bucketName)
                .NotEmpty()
                .WithMessage("bucketName is required");

            RuleFor(x => x.tempKey)
                .NotEmpty()
                .WithMessage("tempKey is required");
        }
    }
}

