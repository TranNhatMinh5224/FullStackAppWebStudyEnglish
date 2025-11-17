using FluentValidation;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Validators.VocabularyReviewValidators;

public class VocabularyReviewDtoValidator : AbstractValidator<VocabularyReviewDto>
{
    public VocabularyReviewDtoValidator()
    {
        RuleFor(x => x.ReviewId)
            .GreaterThan(0)
            .WithMessage("ReviewId phải lớn hơn 0");

        RuleFor(x => x.FlashCardId)
            .GreaterThan(0)
            .WithMessage("FlashCardId phải lớn hơn 0");

        RuleFor(x => x.Quality)
            .InclusiveBetween(0, 5)
            .WithMessage("Quality phải từ 0 đến 5");

        RuleFor(x => x.EasinessFactor)
            .GreaterThan(0)
            .WithMessage("EasinessFactor phải lớn hơn 0");

        RuleFor(x => x.IntervalDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("IntervalDays không được âm");

        RuleFor(x => x.RepetitionCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("RepetitionCount không được âm");

        RuleFor(x => x.NextReviewDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("NextReviewDate phải trong tương lai");

        RuleFor(x => x.ReviewStatus)
            .NotNull()
            .NotEmpty()
            .WithMessage("ReviewStatus không được null hoặc rỗng");
    }
}
