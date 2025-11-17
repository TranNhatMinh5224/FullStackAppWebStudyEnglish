using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.VocabularyReviewValidators;

public class VocabularyReviewResultDtoValidator : AbstractValidator<VocabularyReviewResultDto>
{
    public VocabularyReviewResultDtoValidator()
    {
        RuleFor(x => x.Message)
            .NotNull()
            .WithMessage("Message không được null");

        RuleFor(x => x.NextReviewDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("NextReviewDate phải trong tương lai");

        RuleFor(x => x.NewIntervalDays)
            .GreaterThan(0)
            .WithMessage("NewIntervalDays phải lớn hơn 0");

        RuleFor(x => x.NewEasinessFactor)
            .GreaterThan(0)
            .WithMessage("NewEasinessFactor phải lớn hơn 0");

        RuleFor(x => x.ReviewStatus)
            .NotNull()
            .NotEmpty()
            .WithMessage("ReviewStatus không được null hoặc rỗng");
    }
}
